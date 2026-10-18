using System.Net;
using System.Net.Mail;

namespace Game1.Services;

/// <summary>
/// Sends email through SMTP configured in user-secrets / environment:
/// Smtp:Host, Smtp:Port (587), Smtp:User, Smtp:Password, Smtp:From, Smtp:FromName ("EDU AI").
/// Gmail: Host = smtp.gmail.com, User = From = your Gmail address, Password = a 16-letter
/// app password (Google account → Security → 2-Step Verification → App passwords), not the normal one.
/// Without Smtp:Host / Smtp:Password the message is only written to the log (handy for local development).
/// </summary>
public class EmailSender(IConfiguration config, ILogger<EmailSender> log)
{
    public bool IsConfigured => !string.IsNullOrEmpty(config["Smtp:Host"]) && !string.IsNullOrEmpty(config["Smtp:Password"]);

    /// <param name="text">Plain-text copy of the message — mail without one is more likely to land in spam.</param>
    public async Task SendAsync(string to, string subject, string html, string? text = null)
    {
        if (!IsConfigured)
        {
            log.LogWarning("SMTP is not configured — email to {To} not sent. Subject: {Subject}\n{Body}", to, subject, html);
            return;
        }

        // Google shows app passwords in groups ("abcd efgh ijkl mnop") — the spaces are not part of it.
        var password = (config["Smtp:Password"] ?? "").Replace(" ", "");
        using var client = new SmtpClient(config["Smtp:Host"], int.TryParse(config["Smtp:Port"], out var port) ? port : 587)
        {
            EnableSsl = true,
            Timeout = 15_000,
            Credentials = new NetworkCredential(config["Smtp:User"], password),
        };
        var from = new MailAddress(config["Smtp:From"] ?? config["Smtp:User"]!, config["Smtp:FromName"] ?? "EDU AI");
        using var message = new MailMessage(from, new MailAddress(to))
        {
            Subject = subject,
            SubjectEncoding = System.Text.Encoding.UTF8,
            BodyEncoding = System.Text.Encoding.UTF8,
        };
        if (text != null)
        {
            // multipart/alternative: text first, HTML last (mail apps show the last one they can render).
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, System.Text.Encoding.UTF8, "text/plain"));
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, System.Text.Encoding.UTF8, "text/html"));
        }
        else
        {
            message.Body = html;
            message.IsBodyHtml = true;
        }
        await client.SendMailAsync(message);
        log.LogInformation("Email \"{Subject}\" sent to {To}", subject, to);
    }
}
