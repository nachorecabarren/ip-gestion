using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IpGestion.Application.Common.DTOs;
using IpGestion.Application.Interfaces;

namespace IpGestion.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth, IConfiguration config, IWebHostEnvironment env) : ControllerBase
{
    private void IssueCookie(AuthUserDto user)
    {
        var token = auth.GenerateToken(user);
        var expiryDays = int.Parse(config["Jwt:ExpiryDays"] ?? "7");
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            // SameSite=None requiere Secure; en Development (http://localhost, sin TLS)
            // eso hace que Safari/Firefox descarten la cookie silenciosamente, dejando
            // el login en apariencia "OK" pero sin sesión guardada. Lax alcanza en local
            // porque front y back comparten site (localhost, solo cambia el puerto).
            SameSite = env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Secure = !env.IsDevelopment(),
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
