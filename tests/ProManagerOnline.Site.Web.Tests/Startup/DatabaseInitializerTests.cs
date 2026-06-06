using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Infrastructure.Persistence;
using ProManagerOnline.Site.Web.Startup;

namespace ProManagerOnline.Site.Web.Tests.Startup;

/// <summary>
/// Tests for <see cref="DatabaseInitializer"/>, the start-up step that brings a fresh deployment's
/// database up to date and seeds it. This is what makes a production deploy (not just Development)
/// self-provision its schema, catalogue, and owner admin, so a fresh Azure database is usable on
/// first boot.
/// </summary>
public sealed class DatabaseInitializerTests : IDisposable
{
    private readonly SeedTestHost _host = new();

    [Fact]
    public async Task InitializeAsync_SeedsTheCatalogueAndTheConfiguredAdmin()
    {
        var configuration = ConfigurationWith(("Admin:Email", "owner@example.com"), ("Admin:InitialPassword", "OwnerPw1!"));

        await DatabaseInitializer.InitializeAsync(_host.Services, configuration, applyMigrations: false);

        using var scope = _host.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<SiteDbContext>();
        Assert.True(await database.Products.AnyAsync());

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync("owner@example.com");
        Assert.NotNull(admin);
    }

    [Fact]
    public async Task InitializeAsync_SkipsTheAdmin_WhenCredentialsAreNotConfigured()
    {
        var configuration = ConfigurationWith();

        await DatabaseInitializer.InitializeAsync(_host.Services, configuration, applyMigrations: false);

        using var scope = _host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Empty(userManager.Users);

        // The catalogue is still seeded regardless of whether an admin was configured.
        var database = scope.ServiceProvider.GetRequiredService<SiteDbContext>();
        Assert.True(await database.Products.AnyAsync());
    }

    [Fact]
    public async Task InitializeAsync_IsIdempotent()
    {
        var configuration = ConfigurationWith(("Admin:Email", "owner@example.com"), ("Admin:InitialPassword", "OwnerPw1!"));

        await DatabaseInitializer.InitializeAsync(_host.Services, configuration, applyMigrations: false);
        await DatabaseInitializer.InitializeAsync(_host.Services, configuration, applyMigrations: false);

        using var scope = _host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Single(userManager.Users);
    }

    private static IConfiguration ConfigurationWith(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    public void Dispose() => _host.Dispose();
}
