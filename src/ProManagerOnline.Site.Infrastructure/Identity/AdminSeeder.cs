using Microsoft.AspNetCore.Identity;

namespace ProManagerOnline.Site.Infrastructure.Identity;

/// <summary>
/// Bootstraps the single owner admin account from configured credentials when it does not
/// already exist, so there is always an account to sign in with after a fresh deploy.
/// </summary>
public static class AdminSeeder
{
    /// <summary>
    /// Ensures an admin account with the given email exists. Does nothing if one already does,
    /// so it is safe to call on every start-up.
    /// </summary>
    /// <param name="userManager">The Identity user manager for admin accounts.</param>
    /// <param name="email">The owner admin's email address (also used as the username).</param>
    /// <param name="password">The owner admin's initial password.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the account cannot be created (for example the password fails the configured policy).
    /// </exception>
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var owner = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(owner, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to seed the owner admin account: {errors}");
        }
    }
}
