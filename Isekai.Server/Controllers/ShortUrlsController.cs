using Isekai.Server.Models;
using Isekai.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isekai.Server.Controllers;

[ApiController]
[Route("api/shorturls")]
public class ShortUrlsController(UrlShortenerService urlShortenerService) : ControllerBase
{
    private readonly UrlShortenerService _urlShortenerService =  urlShortenerService;

    [HttpPost]
    public async Task<IActionResult> CreateShortUrl([FromBody] ShortenRequest shortenRequest)
    {
        var code = await _urlShortenerService.ShortenUrl(shortenRequest.Url);
        return Ok(new ShortenResponse(code, $"{Request.Scheme}://{Request.Host}/{code}"));
    }
}