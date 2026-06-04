using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Infrastructure.Tests.Support;
using Xunit;

namespace ProManagerOnline.Site.Infrastructure.Tests;

/// <summary>
/// Integration tests for <see cref="AdminAccountService"/> exercised against a real
/// ASP.NET Core Identity <see cref="UserManager{TUser}"/> over in-memory SQLite.
/// </summary>
public sealed class AdminAccountServiceTests : IDisposable
{
    private readonly IdentityTestHost _host = new();

    [Fact]
    public async Task GeneratePasswordResetToken_ForUnknownEmail_ReturnsNull()
    {
        using var scope = _host.CreateScope();
        var service = new AdminAccountService(UserManager(scope));

        var token = await service.GeneratePasswordResetTokenAsync("nobody@example.com");

        Assert.Null(token);
    }

    [Fact]
    public async Task GeneratePasswordResetToken_ForKnownEmail_ReturnsToken()
    {
        using var scope = _host.CreateScope();
        var userManager = UserManager(scope);
        await CreateAdmin(userManager, "owner@example.com", "OldPw1!");
        var service = new AdminAccountService(userManager);

        var token = await service.GeneratePasswordResetTokenAsync("owner@example.com");

        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public async Task ResetPassword_WithTokenFromService_SetsTheNewPassword()
    {
        using var scope = _host.CreateScope();
        var userManager = UserManager(scope);
        await CreateAdmin(userManager, "owner@example.com", "OldPw1!");
        var service = new AdminAccountService(userManager);

        var token = await service.GeneratePasswordResetTokenAsync("owner@example.com");
        var result = await service.ResetPasswordAsync("owner@example.com", token!, "NewPw1!");

        Assert.True(result.Succeeded);
        var user = await userManager.FindByEmailAsync("owner@example.com");
        Assert.True(await userManager.CheckPasswordAsync(user!, "NewPw1!"));
        Assert.False(await userManager.CheckPasswordAsync(user!, "OldPw1!"));
    }

    [Fact]
    public async Task ResetPassword_WithBadToken_Fails()
    {
        using var scope = _host.CreateScope();
        var userManager = UserManager(scope);
        await CreateAdmin(userManager, "owner@example.com", "OldPw1!");
        var service = new AdminAccountService(userManager);

        var result = await service.ResetPasswordAsync("owner@example.com", "not-a-real-token", "NewPw1!");

        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ChangePassword_WithCorrectCurrentPassword_Succeeds()
    {
        using var scope = _host.CreateScope();
        var userManager = UserManager(scope);
        var user = await CreateAdmin(userManager, "owner@example.com", "OldPw1!");
        var service = new AdminAccountService(userManager);

        var result = await service.ChangePasswordAsync(user.Id, "OldPw1!", "NewPw1!");

        Assert.True(result.Succeeded);
        var reloaded = await userManager.FindByIdAsync(user.Id);
        Assert.True(await userManager.CheckPasswordAsync(reloaded!, "NewPw1!"));
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_Fails()
    {
        using var scope = _host.CreateScope();
        var userManager = UserManager(scope);
        var user = await CreateAdmin(userManager, "owner@example.com", "OldPw1!");
        var service = new AdminAccountService(userManager);

        var result = await service.ChangePasswordAsync(user.Id, "WrongPw!", "NewPw1!");

        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
    }

    private static UserManager<ApplicationUser> UserManager(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    private static async Task<ApplicationUser> CreateAdmin(
        UserManager<ApplicationUser> userManager, string email, string password)
    {
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, password);
        Assert.True(result.Succeeded);
        return user;
    }

    public void Dispose() => _host.Dispose();
}
