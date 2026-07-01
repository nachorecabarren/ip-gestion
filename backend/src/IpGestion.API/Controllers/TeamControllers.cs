using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IpGestion.Application.Common.DTOs;
using IpGestion.Application.Interfaces;

namespace IpGestion.API.Controllers;

// ─── INVITATIONS ───────────────────────────────────────────
[ApiController]
[Route("api/invitations")]
public class InvitationsController(IInvitationService svc, IConfiguration config) : TenantBaseController
{
    // Front-end origin used to build the copy/paste accept link.
    private string FrontendBaseUrl =>
        Request.Headers.Origin.FirstOrDefault()
        ?? config["FrontendUrl"]
        ?? "http://localhost:4200";

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvitationDto dto, CancellationToken ct)
    {
        if (!IsOwner) return Forbid();
        var link = await svc.CreateAsync(TenantId, CurrentUserId, dto.Email, FrontendBaseUrl, ct);
        return Ok(link);
    }

    [HttpGet("validate/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> Validate(Guid token, CancellationToken ct)
    {
        var info = await svc.ValidateTokenAsync(token, ct);
        if (info is null)
            return NotFound(new { error = "La invitación no es válida, ya fue usada o expiró." });
        return Ok(info);
    }

    [HttpPost("accept")]
    [AllowAnonymous]
    public async Task<IActionResult> Accept([FromBody] AcceptInvitationDto dto, IAuthService auth, CancellationToken ct)
    {
        var user = await svc.AcceptAsync(dto, ct);

        // Auto-login the freshly created employee (same HttpOnly cookie flow as login)
        var jwt = auth.GenerateToken(user);
        var expiryDays = int.Parse(config["Jwt:ExpiryDays"] ?? "7");
        Response.Cookies.Append("jwt", jwt, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = false,
            Expires = DateTimeOffset.UtcNow.AddDays(expiryDays),
            Path = "/",
        });

        return Ok(new { userId = user.UserId, tenantId = user.TenantId, email = user.Email, displayName = user.DisplayName, role = user.Role });
    }

    [HttpGet]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        if (!IsOwner) return Forbid();
        return Ok(await svc.GetPendingAsync(TenantId, ct));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        if (!IsOwner) return Forbid();
        await svc.CancelAsync(TenantId, id, ct);
        return NoContent();
    }
}

// ─── USERS (TEAM) ──────────────────────────────────────────
[ApiController]
[Route("api/usuarios")]
public class UsersController(IUserService svc) : TenantBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        if (!IsOwner) return Forbid();
        return Ok(await svc.GetUsersAsync(TenantId, ct));
    }

    [HttpPut("{id}/desactivar")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        if (!IsOwner) return Forbid();
        await svc.DeactivateAsync(TenantId, id, CurrentUserId, ct);
        return NoContent();
    }
}
