using ProManagerOnline.Site.Application.Email;

namespace ProManagerOnline.Site.Application.Administration;

/// <summary>Input for requesting a password-reset email for an admin account.</summary>
/// <param name="Email">The email address the admin claims to own.</param>
public sealed record RequestPasswordResetCommand(string Email);

/// <summary>
/// Starts the forgot-password flow: if an admin with the given email exists, generates a
/// reset token, builds a reset link from it and emails the link. When no such admin exists
/// it silently does nothing, so callers cannot use this to discover which emails are registered.
/// </summary>
/// <param name="accounts">The admin account service.</param>
/// <param name="emailSender">The email sender used to deliver the reset link.</param>
public sealed class RequestPasswordResetHandler(IAdminAccountService accounts, IEmailSender emailSender)
{
    /// <summary>Sends a password-reset email when the account exists.</summary>
    /// <param name="command">The email address to send the reset link to.</param>
    /// <param name="resetLinkFactory">
    /// Builds the absolute reset link from a reset token. Supplied by the presentation layer,
    /// which alone knows the request's scheme and host and how to encode the token into a URL.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task Handle(
        RequestPasswordResetCommand command,
        Func<string, string> resetLinkFactory,
        CancellationToken cancellationToken = default)
    {
        var token = await accounts.GeneratePasswordResetTokenAsync(command.Email, cancellationToken);
        if (token is null)
        {
            return;
        }

        var resetLink = resetLinkFactory(token);
        var body =
            "<p>We received a request to reset your ProManager Online admin password.</p>" +
            $"<p><a href=\"{resetLink}\">Reset your password</a></p>" +
            "<p>If you did not request this, you can safely ignore this email.</p>";

        await emailSender.SendAsync(command.Email, "Reset your ProManager Online admin password", body, cancellationToken);
    }
}
