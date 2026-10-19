using Isekai.Data;
using Isekai.Data.Repositories;
using Isekai.Server.Models;
using StackExchange.Redis;

namespace Isekai.Server.Services;

public class UrlShortenerService(IShortUrlRepository repository, IConnectionMultiplexer connectionMux, ClickBuffer clicks)
{

    public async Task<string> ShortenUrl(string url, string? title = null, long? userId = null)
    {
        var id = await repository.CreateAsync(new ShortUrl
        {
            LongUrl = url,
            CreatedAt = DateTime.UtcNow,
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            UserId = userId
        });
        return Base62.Encode(id);
    }

    public async Task<string?> ResolveAsync(string code)
    {
        var url = await GetUrlAsync(code);
        if (url is not null)
            clicks.Record(code);
        return url;
    }
    
    public async Task<bool> DeleteAsync(string code, long userId)
    {
        var deleted = await repository.DeleteAsync(code, userId);
        if (!deleted) return deleted;
        var db = connectionMux.GetDatabase();
        await db.KeyDeleteAsync(CacheKey(code));
        return deleted;
    }

    public async Task<List<ShortUrl>> GetAllShortUrls(long userId)
    {
        return [.. await repository.GetAllAsync(userId)];
    }

    private async Task<string?> GetUrlAsync(string code)
    {
        var db = connectionMux.GetDatabase();
        var cacheKey = CacheKey(code);
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
    
    private static string CacheKey(string code) => $"url:{code}";
}
