using Microsoft.AspNetCore.Identity;
using ProManagerOnline.Site.Application.Administration;

namespace ProManagerOnline.Site.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity implementation of <see cref="IAdminAccountService"/>. Wraps
/// <see cref="UserManager{TUser}"/> and maps Identity's results onto the application's
/// <see cref="AdminAccountResult"/>, so no Identity types leak past the infrastructure boundary.
/// </summary>
/// <param name="userManager">The Identity user manager for admin accounts.</param>
public sealed class AdminAccountService(UserManager<ApplicationUser> userManager) : IAdminAccountService
{
    /// <inheritdoc />
    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is null ? null : await userManager.GeneratePasswordResetTokenAsync(user);
    }

    /// <inheritdoc />
    public async Task<AdminAccountResult> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            // Mirror Identity's own behaviour and avoid revealing whether the email is registered.
            return AdminAccountResult.Failure("Invalid password reset request.");
        }

        return ToResult(await userManager.ResetPasswordAsync(user, token, newPassword));
    }

    /// <inheritdoc />
    public async Task<AdminAccountResult> ChangePasswordAsync(
        string adminId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(adminId);
        if (user is null)
        {
            return AdminAccountResult.Failure("Account not found.");
        }

        return ToResult(await userManager.ChangePasswordAsync(user, currentPassword, newPassword));
    }

    private static AdminAccountResult ToResult(IdentityResult result)
        => result.Succeeded
            ? AdminAccountResult.Success()
            : AdminAccountResult.Failure(result.Errors.Select(error => error.Description));
}
