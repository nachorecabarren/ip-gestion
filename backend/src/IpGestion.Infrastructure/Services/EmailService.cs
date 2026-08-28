using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IpGestion.Application.Interfaces;

namespace IpGestion.Infrastructure.Services;

// ─── EMAIL SERVICE (Resend) ─────────────────────────────────
// Best-effort: a failed send is logged but never throws, so invitation
// creation never breaks because of an email provider issue.
public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    private const string ResendEndpoint = "https://api.resend.com/emails";

    public Task SendInvitationAsync(string toEmail, string businessName, string invitationLink, CancellationToken ct = default)
        => SendAsync(toEmail, $"Te invitaron a unirte a {businessName} en iP Gestión",
            BuildInvitationHtml(businessName, invitationLink), ct);

    public Task SendServiceReadyAsync(string toEmail, string clientName, string svCode, string? deviceModel, CancellationToken ct = default)
        => SendAsync(toEmail, $"Tu equipo está listo para retirar — Orden {svCode}",
            BuildServiceReadyHtml(clientName, svCode, deviceModel), ct);

    public Task SendSaleInvoiceAsync(string toEmail, string clientName, string saleCode, DateTime saleDate, List<string> itemLines, decimal totalUsd, string paymentsSummary, int warrantyDays, CancellationToken ct = default)
        => SendAsync(toEmail, $"Tu comprobante de compra — {saleCode}",
            BuildSaleInvoiceHtml(clientName, saleCode, saleDate, itemLines, totalUsd, paymentsSummary, warrantyDays), ct);

    private async Task SendAsync(string toEmail, string subject, string html, CancellationToken ct)
    {
        var apiKey = config["Resend:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Resend:ApiKey no está configurada; no se envió el email a {Email}.", toEmail);
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                from = "iP Gestión <onboarding@resend.dev>",
                to = new[] { toEmail },
                subject,
                html,
            };

            var response = await client.PostAsJsonAsync(ResendEndpoint, payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                logger.LogError(
                    "Resend respondió {StatusCode} al enviar un email a {Email}: {Body}",
                    response.StatusCode, toEmail, body);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enviando un email a {Email}.", toEmail);
        }
    }

    private static string BuildInvitationHtml(string businessName, string invitationLink) => $"""
        <!DOCTYPE html>
        <html>
          <body style="margin:0;padding:0;background-color:#f8fafc;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#f8fafc;padding:32px 16px;">
              <tr>
                <td align="center">
                  <table role="presentation" width="100%" style="max-width:480px;background-color:#ffffff;border-radius:12px;border:1px solid #e2e8f0;">
                    <tr>
                      <td style="padding:32px 32px 8px;text-align:center;">
                        <span style="font-size:20px;font-weight:800;color:#2563eb;letter-spacing:-0.5px;">iP Gestión</span>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:16px 32px 8px;text-align:center;">
                        <p style="margin:0;font-size:15px;line-height:1.5;color:#0f172a;">
                          <strong>{businessName}</strong> te invitó a unirte a su equipo en iP Gestión.
                        </p>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:24px 32px;text-align:center;">
                        <a href="{invitationLink}" style="display:inline-block;background-color:#2563eb;color:#ffffff;text-decoration:none;font-size:14px;font-weight:600;padding:12px 28px;border-radius:8px;">
                          Aceptar invitación
                        </a>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:0 32px 8px;text-align:center;">
                        <p style="margin:0;font-size:12px;color:#64748b;">Este link expira en 7 días.</p>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:24px 32px 32px;text-align:center;border-top:1px solid #f1f5f9;">
                        <p style="margin:16px 0 0;font-size:11px;color:#94a3b8;">
                          Si no esperabas esta invitación, podés ignorar este email.
                        </p>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </body>
        </html>
        """;

    private static string BuildServiceReadyHtml(string clientName, string svCode, string? deviceModel) => $"""
        <!DOCTYPE html>
        <html>
          <body style="margin:0;padding:0;background-color:#f8fafc;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#f8fafc;padding:32px 16px;">
              <tr>
                <td align="center">
                  <table role="presentation" width="100%" style="max-width:480px;background-color:#ffffff;border-radius:12px;border:1px solid #e2e8f0;">
                    <tr>
                      <td style="padding:32px 32px 8px;text-align:center;">
                        <span style="font-size:20px;font-weight:800;color:#2563eb;letter-spacing:-0.5px;">iP Gestión</span>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:16px 32px 8px;text-align:center;">
                        <p style="margin:0;font-size:15px;line-height:1.5;color:#0f172a;">
                          Hola <strong>{clientName}</strong>, tu equipo{(deviceModel is null ? "" : $" ({deviceModel})")} ya está listo para retirar.
                        </p>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:8px 32px 24px;text-align:center;">
                        <p style="margin:0;font-size:13px;color:#64748b;">Orden de servicio: <strong>{svCode}</strong></p>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:0 32px 32px;text-align:center;border-top:1px solid #f1f5f9;">
                        <p style="margin:16px 0 0;font-size:11px;color:#94a3b8;">
                          Cualquier consulta, respondé este email o contactanos directamente.
                        </p>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </body>
        </html>
        """;

    private static string BuildSaleInvoiceHtml(string clientName, string saleCode, DateTime saleDate, List<string> itemLines, decimal totalUsd, string paymentsSummary, int warrantyDays)
    {
        var rows = string.Join("", itemLines.Select(line => $"""
            <tr>
              <td style="padding:8px 0;font-size:13px;color:#0f172a;border-bottom:1px solid #f1f5f9;">{line}</td>
            </tr>
            """));

        return $"""
        <!DOCTYPE html>
        <html>
          <body style="margin:0;padding:0;background-color:#f8fafc;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#f8fafc;padding:32px 16px;">
              <tr>
                <td align="center">
                  <table role="presentation" width="100%" style="max-width:480px;background-color:#ffffff;border-radius:12px;border:1px solid #e2e8f0;">
                    <tr>
                      <td style="padding:32px 32px 8px;text-align:center;">
                        <span style="font-size:20px;font-weight:800;color:#2563eb;letter-spacing:-0.5px;">iP Gestión</span>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:16px 32px 0;text-align:center;">
                        <p style="margin:0;font-size:15px;line-height:1.5;color:#0f172a;">
                          Hola <strong>{clientName}</strong>, gracias por tu compra.
                        </p>
                        <p style="margin:4px 0 0;font-size:12px;color:#64748b;">
                          Comprobante {saleCode} · {saleDate:dd/MM/yyyy}
                        </p>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:20px 32px 0;">
                        <table role="presentation" width="100%" style="border-top:1px solid #f1f5f9;">
                          {rows}
                        </table>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:12px 32px 0;text-align:right;">
                        <p style="margin:0;font-size:16px;font-weight:700;color:#0f172a;">Total: u$d {totalUsd:0.00}</p>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:8px 32px 0;text-align:right;">
                        <p style="margin:0;font-size:12px;color:#64748b;">{paymentsSummary}</p>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:16px 32px 32px;text-align:center;border-top:1px solid #f1f5f9;margin-top:16px;">
                        <p style="margin:16px 0 0;font-size:12px;color:#64748b;">Garantía: {warrantyDays} días.</p>
                        <p style="margin:8px 0 0;font-size:11px;color:#94a3b8;">
                          Cualquier consulta, respondé este email o contactanos directamente.
                        </p>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </body>
        </html>
        """;
    }
}
