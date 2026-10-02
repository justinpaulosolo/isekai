using System.Security.Claims;
using Isekai.Server.Models;
using Isekai.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isekai.Server.Controllers;

[ApiController]
[Route("api/shorturls")]
public class ShortUrlsController(UrlShortenerService urlShortenerService) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> CreateShortUrl([FromBody] ShortenRequest shortenRequest)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        long? userId = long.TryParse(userIdClaim, out var id) ? id : null;
        var code = await urlShortenerService.ShortenUrl(shortenRequest.Url, shortenRequest.Title, userId);
        return Ok(new ShortenResponse(code, $"{Request.Scheme}://{Request.Host}/{code}"));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllShortUrls()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(id, out var userId))
        {
            // Conversion succeeded, 'result' contains the long value
            var urls = await urlShortenerService.GetAllShortUrls(userId);
            return Ok(urls);
        }
        return BadRequest();
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteShortUrl(string code)
    {
        await urlShortenerService.DeleteAsync(code);
        return NoContent();
    }
}