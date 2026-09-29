using Isekai.Server.Models;
using Isekai.Server.Repositories;
using StackExchange.Redis;

namespace Isekai.Server.Services;

public class UrlShortenerService(IShortUrlRepository repository, IConnectionMultiplexer connectionMux)
{

    public async Task<string> ShortenUrl(string url)
    {
        var id = await repository.CreateAsync(new ShortUrl { LongUrl = url, CreatedAt = DateTime.UtcNow });
        return Base62.Encode(id);
    }

    public async Task<string?> ResolveAsync(string code)
    {
        var db = connectionMux.GetDatabase();
        var cacheKey = $"url:{code}";
        var lockKey = $"lock:url:{code}";
        
        var cached = await db.StringGetAsync(code);
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
}
