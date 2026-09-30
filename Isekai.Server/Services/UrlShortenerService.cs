using System.Text;
using System.Text.Json;
using Isekai.Data;
using Isekai.Data.Repositories;
using Isekai.Server.Models;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Isekai.Server.Services;

public class UrlShortenerService(IShortUrlRepository repository, IConnectionMultiplexer connectionMux, IConnection connection)
{

    public async Task<string> ShortenUrl(string url, string? title = null)
    {
        var id = await repository.CreateAsync(new ShortUrl
        {
            LongUrl = url,
            CreatedAt = DateTime.UtcNow,
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim()
        });
        return Base62.Encode(id);
    }

    public async Task<string?> ResolveAsync(string code)
    {
        await using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(
            queue: "click",
            durable: true,
            exclusive: false,
            autoDelete: false);
        var eventId = Guid.NewGuid().ToString("N");
        
        var json = JsonSerializer.Serialize(new
        {
            eventId,
            code,
            occurredOn = DateTime.UtcNow
        });
        
        var body = Encoding.UTF8.GetBytes(json);
        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = eventId,
        };
        
        await channel.BasicPublishAsync(
            exchange:"",
            routingKey: "click",
            mandatory: false,
            basicProperties: props,
            body: body);
        
        var db = connectionMux.GetDatabase();
        var cacheKey = $"url:{code}";
        var lockKey = $"lock:url:{code}";
        
        var cached = await db.StringGetAsync(cacheKey);
        if (!cached.IsNullOrEmpty)
            return cached;
        
        var lockToken = Guid.NewGuid().ToString("N");
        var gotLock = await db.LockTakeAsync(lockKey, lockToken, TimeSpan.FromSeconds(5));

        if (gotLock)
        {
            try
            {
                cached = await db.StringGetAsync(cacheKey);
                if (!cached.IsNullOrEmpty)
                    return cached;
                
                var shortUrl = await repository.GetByCodeAsync(code);
                if (shortUrl is null)
                    return null;
        
                await db.StringSetAsync(cacheKey, shortUrl.LongUrl, TimeSpan.FromHours(1));
                return shortUrl.LongUrl;

            }
            finally
            {
                await db.LockReleaseAsync(lockKey, lockToken);
            }
        }

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await Task.Delay(50);
            cached = await db.StringGetAsync(cacheKey);
            if (!cached.IsNullOrEmpty)
                return cached;
        }

        var fallback = await repository.GetByCodeAsync(code);
        return fallback?.LongUrl;
    }
    
    public async Task DeleteAsync(string code)
    {
        await repository.DeleteAsync(code);
        var db = connectionMux.GetDatabase();
        await db.KeyDeleteAsync($"url:{code}");
    }
}
