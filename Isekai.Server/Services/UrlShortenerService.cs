using Isekai.Server.Models;
using Isekai.Server.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Isekai.Server.Services;

public class UrlShortenerService(IShortUrlRepository repository, IMemoryCache  memoryCache)
{

    public async Task<string> ShortenUrl(string url)
    {
        var id = await repository.CreateAsync(new ShortUrl { LongUrl = url, CreatedAt = DateTime.UtcNow });
        return Base62.Encode(id);
    }

    public async Task<string?> ResolveAsync(string code)
    {
        if (memoryCache.TryGetValue(code, out string? longUrl))
            return longUrl;
        
        var shortUrl = await repository.GetByCodeAsync(code);
        if (shortUrl is null) return null;
        
        memoryCache.Set(code, shortUrl.LongUrl, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(60)
        });
        
        return shortUrl.LongUrl;
    }
}
