using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Infrastructure.Persistence;

namespace ProManagerOnline.Site.Infrastructure.Tests.Support;

/// <summary>
/// Spins up a minimal ASP.NET Core Identity stack (UserManager, password hashing, data
/// protection token providers) over an in-memory SQLite <see cref="SiteDbContext"/>, so the
/// admin account service and seeder can be exercised against the real framework. Mirrors the
/// connection-kept-open SQLite pattern used by the product repository tests.
/// </summary>
internal sealed class IdentityTestHost : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public IdentityTestHost()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddDbContext<SiteDbContext>(options => options.UseSqlite(_connection));
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Relaxed password rules keep the test fixtures readable; lockout matches production.
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<SiteDbContext>()
            .AddDefaultTokenProviders();

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<SiteDbContext>().Database.EnsureCreated();
    }

    /// <summary>Creates a DI scope from which to resolve the scoped <c>UserManager</c>.</summary>
    public IServiceScope CreateScope() => _provider.CreateScope();

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
