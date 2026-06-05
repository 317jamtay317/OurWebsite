namespace ProManagerOnline.Site.Application.Administration;

/// <summary>Input for changing the signed-in admin's password.</summary>
/// <param name="AdminId">The admin's identifier, taken from the signed-in principal.</param>
/// <param name="CurrentPassword">The admin's existing password, for verification.</param>
/// <param name="NewPassword">The new password to set.</param>
public sealed record ChangeAdminPasswordCommand(string AdminId, string CurrentPassword, string NewPassword);

/// <summary>
/// Changes the password of the currently signed-in admin, verifying the current password first.
/// </summary>
/// <param name="accounts">The admin account service.</param>
public sealed class ChangeAdminPasswordHandler(IAdminAccountService accounts)
{
    /// <summary>Changes the admin's password.</summary>
    /// <param name="command">The current and new passwords, with the admin's id.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or a failure carrying validation errors.</returns>
    public Task<AdminAccountResult> Handle(ChangeAdminPasswordCommand command, CancellationToken cancellationToken = default)
        => accounts.ChangePasswordAsync(command.AdminId, command.CurrentPassword, command.NewPassword, cancellationToken);
}
