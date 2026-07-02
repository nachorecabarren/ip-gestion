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

    public async Task SendInvitationAsync(string toEmail, string businessName, string invitationLink, CancellationToken ct = default)
    {
        var apiKey = config["Resend:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Resend:ApiKey no está configurada; no se envió el email de invitación a {Email}.", toEmail);
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                from = "iP Gestión <noreply@notification.ipgestion>",
                to = new[] { toEmail },
                subject = $"Te invitaron a unirte a {businessName} en iP Gestión",
                html = BuildInvitationHtml(businessName, invitationLink),
            };

            var response = await client.PostAsJsonAsync(ResendEndpoint, payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                logger.LogError(
                    "Resend respondió {StatusCode} al enviar la invitación a {Email}: {Body}",
                    response.StatusCode, toEmail, body);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enviando el email de invitación a {Email}.", toEmail);
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
}
