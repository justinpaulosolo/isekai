using Isekai.Server.Models;
using Isekai.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isekai.Server.Controllers;

[ApiController]
[Route("api/shorturls")]
public class ShortUrlsController(UrlShortenerService urlShortenerService) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> CreateShortUrl([FromBody] ShortenRequest shortenRequest)
    {
        var code = await urlShortenerService.ShortenUrl(shortenRequest.Url, shortenRequest.Title);
        return Ok(new ShortenResponse(code, $"{Request.Scheme}://{Request.Host}/{code}"));
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteShortUrl(string code)
    {
        await urlShortenerService.DeleteAsync(code);
        return NoContent();
    }
}