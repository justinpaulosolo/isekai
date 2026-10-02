using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isekai.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AccountController : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        // Only allow local redirects (prevents open-redirect)
        var target = !string.IsNullOrEmpty(returnUrl)
                     && returnUrl.StartsWith('/')
                     && !returnUrl.StartsWith("//")
                     && !returnUrl.StartsWith("/\\")
            ? returnUrl : "/";

        return  Challenge(
            new AuthenticationProperties { RedirectUri = target },
            [GoogleDefaults.AuthenticationScheme]);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
    
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            name = User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(ClaimTypes.Email),
            picture = User.FindFirstValue("picture")
        });
    }
}