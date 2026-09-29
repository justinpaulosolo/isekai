using Isekai.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isekai.Server.Controllers;

[ApiController]
[Route("")]
public class RedirectController(UrlShortenerService service) : ControllerBase
{
    private readonly UrlShortenerService _service = service;

    [HttpGet("{code}")]
    public async Task<IActionResult> ResolveAndRedirect(string code)
    {
        var longUrl = await _service.ResolveAsync(code);
        return longUrl is null ? NotFound() : RedirectPermanent(longUrl);
    }
}
