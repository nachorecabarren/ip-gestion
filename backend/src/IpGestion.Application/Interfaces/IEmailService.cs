namespace IpGestion.Application.Interfaces;

public interface IEmailService
{
    Task SendInvitationAsync(string toEmail, string businessName, string invitationLink, CancellationToken ct = default);
    Task SendServiceReadyAsync(string toEmail, string clientName, string svCode, string? deviceModel, CancellationToken ct = default);
    Task SendSaleInvoiceAsync(string toEmail, string clientName, string saleCode, DateTime saleDate, List<string> itemLines, decimal totalUsd, string paymentsSummary, int warrantyDays, CancellationToken ct = default);
}
