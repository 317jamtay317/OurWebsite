using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using ProManagerOnline.Site.Application.Email;

namespace ProManagerOnline.Site.Infrastructure.Email;

/// <summary>
/// <see cref="IEmailSender"/> that delivers mail over SMTP using the configured server. Registered
/// in place of the development logging sender once an SMTP host is configured.
/// </summary>
/// <param name="options">The SMTP configuration.</param>
public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    /// <inheritdoc />
    public async Task SendAsync(
        string recipient, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        using var message = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(recipient);

        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.UseSsl,
            Credentials = new NetworkCredential(settings.User, settings.Password),
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}
