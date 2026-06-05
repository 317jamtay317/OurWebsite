using Microsoft.Extensions.Logging;
using ProManagerOnline.Site.Application.Email;

namespace ProManagerOnline.Site.Infrastructure.Email;

/// <summary>
/// Development <see cref="IEmailSender"/> that writes messages to the application log instead
/// of delivering real email. It lets the password-reset flow run end to end before an SMTP
/// provider is configured: the reset link appears in the server log. Replace the registration
/// in <c>AddInfrastructure</c> with a real SMTP-backed sender before going live.
/// </summary>
/// <param name="logger">The logger that captures the would-be email.</param>
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    /// <inheritdoc />
    public Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "DEV EMAIL (not delivered) — To: {Recipient}; Subject: {Subject}\n{HtmlBody}",
            recipient,
            subject,
            htmlBody);

        return Task.CompletedTask;
    }
}
