using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Infrastructure.Tests.Support;
using Xunit;

namespace ProManagerOnline.Site.Infrastructure.Tests;

/// <summary>Integration tests for <see cref="AdminSeeder"/>.</summary>
public sealed class AdminSeederTests : IDisposable
{
    private readonly IdentityTestHost _host = new();

    [Fact]
    public async Task SeedAsync_CreatesAConfirmedOwnerThatCanAuthenticate()
    {
        using var scope = _host.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await AdminSeeder.SeedAsync(userManager, "owner@example.com", "OwnerPw1!");

        var user = await userManager.FindByEmailAsync("owner@example.com");
        Assert.NotNull(user);
        Assert.True(user!.EmailConfirmed);
        Assert.True(await userManager.CheckPasswordAsync(user, "OwnerPw1!"));
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        using var scope = _host.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await AdminSeeder.SeedAsync(userManager, "owner@example.com", "OwnerPw1!");
        await AdminSeeder.SeedAsync(userManager, "owner@example.com", "OwnerPw1!");

        Assert.Single(userManager.Users.Where(u => u.Email == "owner@example.com"));
    }

    public void Dispose() => _host.Dispose();
}
