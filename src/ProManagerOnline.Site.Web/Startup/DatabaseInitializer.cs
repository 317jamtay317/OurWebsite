using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Infrastructure.Persistence;

namespace ProManagerOnline.Site.Web.Startup;

/// <summary>
/// Brings the site database up to date and seeds it on application start-up, so a fresh deployment
/// — local <i>or</i> in the cloud — is usable on first boot without any manual database step. It
/// applies the EF Core migrations, seeds the product catalogue, and creates the owner admin account
/// from configuration when credentials are supplied. Every step is idempotent, so it is safe to run
/// on every start.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Runs the migration and seeding steps within a fresh service scope.
    /// </summary>
    /// <param name="services">The root service provider; a scope is created internally.</param>
    /// <param name="configuration">
    /// Application configuration. The owner admin is seeded only when both <c>Admin:Email</c> and
    /// <c>Admin:InitialPassword</c> are present.
    /// </param>
    /// <param name="applyMigrations">
    /// When <see langword="true"/> the EF Core migrations are applied (the production and local
    /// behaviour). Pass <see langword="false"/> when the schema is provisioned externally — for
    /// example in tests that create the schema directly because the migrations target SQL Server.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        bool applyMigrations = true,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var database = provider.GetRequiredService<SiteDbContext>();
        if (applyMigrations)
        {
            await database.Database.MigrateAsync(cancellationToken);
        }

        await SiteDbSeeder.SeedAsync(database, cancellationToken);

        var adminEmail = configuration["Admin:Email"];
        var adminPassword = configuration["Admin:InitialPassword"];
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            await AdminSeeder.SeedAsync(userManager, adminEmail, adminPassword, cancellationToken);
        }
    }
}
