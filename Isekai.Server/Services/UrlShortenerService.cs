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
        
        var cached = await db.StringGetAsync(code);
        
        if (!cached.IsNullOrEmpty)
            return cached;
        
        var shortUrl = await repository.GetByCodeAsync(code);
        if (shortUrl is null) return null;
        
        await db.StringSetAsync(code, shortUrl.LongUrl, TimeSpan.FromHours(1));
        
        return shortUrl.LongUrl;
    }
}
