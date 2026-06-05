namespace ProManagerOnline.Site.Application.Administration;

/// <summary>
/// Abstraction over the admin account store for the operations that do not need an
/// <c>HttpContext</c> (generating and redeeming password-reset tokens, changing a
/// password). Defined in the application layer and implemented in infrastructure over
/// ASP.NET Core Identity. Interactive cookie sign-in/out is handled separately in the
/// presentation layer.
/// </summary>
public interface IAdminAccountService
{
    /// <summary>
    /// Generates a password-reset token for the admin with the given email, if one exists.
    /// </summary>
    /// <param name="email">The admin's email address.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The reset token, or <see langword="null"/> when no admin account uses that email.
    /// The caller must not reveal which of these occurred, to avoid account enumeration.
    /// </returns>
    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets an admin's password using a token previously issued by
    /// <see cref="GeneratePasswordResetTokenAsync"/>.
    /// </summary>
    /// <param name="email">The admin's email address.</param>
    /// <param name="token">The reset token from the emailed link.</param>
    /// <param name="newPassword">The new password to set.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or a failure carrying validation errors.</returns>
    Task<AdminAccountResult> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the password of the currently signed-in admin, verifying their current password.
    /// </summary>
    /// <param name="adminId">The admin's identifier (from the signed-in principal).</param>
    /// <param name="currentPassword">The admin's existing password.</param>
    /// <param name="newPassword">The new password to set.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or a failure carrying validation errors.</returns>
    Task<AdminAccountResult> ChangePasswordAsync(
        string adminId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);
}
