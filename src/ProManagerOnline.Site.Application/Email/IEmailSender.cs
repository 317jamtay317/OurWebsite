namespace ProManagerOnline.Site.Application.Email;

/// <summary>
/// Sends transactional emails (for example a password-reset link). Defined in the
/// application layer and implemented in infrastructure, so use cases can send mail
/// without depending on a concrete mail provider.
/// </summary>
public interface IEmailSender
{
    /// <summary>Sends an email message.</summary>
    /// <param name="recipient">The destination email address.</param>
    /// <param name="subject">The message subject line.</param>
    /// <param name="htmlBody">The message body, as HTML.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
