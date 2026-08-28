namespace IpGestion.Application.Interfaces;

public interface IEmailService
{
    Task SendInvitationAsync(string toEmail, string businessName, string invitationLink, CancellationToken ct = default);
    Task SendServiceReadyAsync(string toEmail, string clientName, string svCode, string? deviceModel, CancellationToken ct = default);
}
