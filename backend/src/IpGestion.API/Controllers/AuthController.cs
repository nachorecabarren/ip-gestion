using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IpGestion.Application.Common.DTOs;
using IpGestion.Application.Interfaces;

namespace IpGestion.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth, IConfiguration config) : ControllerBase
{
    private void IssueCookie(AuthUserDto user)
    {
        var token = auth.GenerateToken(user);
        var expiryDays = int.Parse(config["Jwt:ExpiryDays"] ?? "7");
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true,   // set to true when deploying behind HTTPS
            Expires = DateTimeOffset.UtcNow.AddDays(expiryDays),
            Path = "/",
        });
    }

    private static object Shape(AuthUserDto u) =>
        new { userId = u.UserId, tenantId = u.TenantId, email = u.Email, displayName = u.DisplayName, role = u.Role };

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto, CancellationToken ct)
    {
        var user = await auth.RegisterAsync(dto, ct);
        IssueCookie(user);
        return Ok(Shape(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var user = await auth.ValidateAsync(dto.Email, dto.Password, ct);
        if (user is null)
            return Unauthorized(new { error = "Email o contraseña incorrectos" });

        IssueCookie(user);
        return Ok(Shape(user));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt", new CookieOptions { Path = "/" });
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = User.FindFirstValue("userId");
        if (userId is null || !Guid.TryParse(userId, out var id))
            return Unauthorized();

        var user = await auth.GetByIdAsync(id, ct);
        if (user is null) return Unauthorized();

        return Ok(Shape(user));
    }
}
