using System.Net;
using System.Net.Mail;

namespace Game1.Services;

/// <summary>
/// Sends email through SMTP configured in user-secrets / environment:
/// Smtp:Host, Smtp:Port (587), Smtp:User, Smtp:Password, Smtp:From.
/// Without Smtp:Host the message is only written to the log (handy for local development).
/// </summary>
public class EmailSender(IConfiguration config, ILogger<EmailSender> log)
{
    public bool IsConfigured => !string.IsNullOrEmpty(config["Smtp:Host"]);

    public async Task SendAsync(string to, string subject, string html)
    {
        if (!IsConfigured)
        {
            log.LogWarning("SMTP is not configured — email to {To} not sent. Subject: {Subject}\n{Body}", to, subject, html);
            return;
        }

        using var client = new SmtpClient(config["Smtp:Host"], int.TryParse(config["Smtp:Port"], out var port) ? port : 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(config["Smtp:User"], config["Smtp:Password"]),
        };
        using var message = new MailMessage(config["Smtp:From"] ?? config["Smtp:User"]!, to, subject, html) { IsBodyHtml = true };
        await client.SendMailAsync(message);
    }
}
