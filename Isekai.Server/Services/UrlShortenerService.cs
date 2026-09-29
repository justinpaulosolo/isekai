using Isekai.Server.Models;
using Isekai.Server.Repositories;

namespace Isekai.Server.Services;

public class UrlShortenerService(IShortUrlRepository repository)
{
    private readonly IShortUrlRepository _repository = repository;

    public async Task<string> ShortenUrl(string url)
    {
        var id = await _repository.CreateAsync(new ShortUrl { LongUrl = url, CreatedAt = DateTime.UtcNow });
        return Base62.Encode(id);
    }

    public async Task<string?> ResolveAsync(string code)
    {
        var url =  await _repository.GetByCodeAsync(code);
        return url?.LongUrl;
    }
}
