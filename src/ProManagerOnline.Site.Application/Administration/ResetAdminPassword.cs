namespace ProManagerOnline.Site.Application.Administration;

/// <summary>Input for resetting an admin's password using an emailed reset token.</summary>
/// <param name="Email">The admin's email address.</param>
/// <param name="Token">The reset token from the emailed link.</param>
/// <param name="NewPassword">The new password to set.</param>
public sealed record ResetAdminPasswordCommand(string Email, string Token, string NewPassword);

/// <summary>
/// Resets an admin's password using a token previously issued by
/// <see cref="RequestPasswordResetHandler"/>.
/// </summary>
/// <param name="accounts">The admin account service.</param>
public sealed class ResetAdminPasswordHandler(IAdminAccountService accounts)
{
    /// <summary>Resets the admin's password.</summary>
    /// <param name="command">The email, reset token and new password.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or a failure carrying validation errors.</returns>
    public Task<AdminAccountResult> Handle(ResetAdminPasswordCommand command, CancellationToken cancellationToken = default)
        => accounts.ResetPasswordAsync(command.Email, command.Token, command.NewPassword, cancellationToken);
}
