using ProManagerOnline.Site.Application.Administration;

namespace ProManagerOnline.Site.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IAdminAccountService"/> test double. Returns configurable results
/// and records the arguments of the last call, so handler tests can run without ASP.NET
/// Core Identity or a database.
/// </summary>
internal sealed class FakeAdminAccountService : IAdminAccountService
{
    public string? ResetTokenToReturn { get; set; }

    public AdminAccountResult ResetResult { get; set; } = AdminAccountResult.Success();

    public AdminAccountResult ChangeResult { get; set; } = AdminAccountResult.Success();

    public string? LastGenerateTokenEmail { get; private set; }

    public (string Email, string Token, string NewPassword)? LastReset { get; private set; }

    public (string AdminId, string CurrentPassword, string NewPassword)? LastChange { get; private set; }

    public Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        LastGenerateTokenEmail = email;
        return Task.FromResult(ResetTokenToReturn);
    }

    public Task<AdminAccountResult> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        LastReset = (email, token, newPassword);
        return Task.FromResult(ResetResult);
    }

    public Task<AdminAccountResult> ChangePasswordAsync(
        string adminId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        LastChange = (adminId, currentPassword, newPassword);
        return Task.FromResult(ChangeResult);
    }
}
