using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Infrastructure.Persistence;

namespace ProManagerOnline.Site.Web.Tests.Startup;

/// <summary>
/// Builds a minimal service provider — an in-memory SQLite <see cref="SiteDbContext"/> plus the
/// ASP.NET Core Identity core stack — so the startup database initializer can be exercised against
/// the real seeders without a SQL Server. Mirrors the connection-kept-open SQLite pattern used by
/// the infrastructure tests; the schema is created with <c>EnsureCreated</c> because the EF
/// migrations target SQL Server.
/// </summary>
internal sealed class SeedTestHost : IDisposable
{
    private readonly SqliteConnection _connection;

    public SeedTestHost()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddDbContext<SiteDbContext>(options => options.UseSqlite(_connection));
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Relaxed password rules keep the fixtures readable; production rules live in Program.cs.
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<SiteDbContext>()
            .AddDefaultTokenProviders();

        Services = services.BuildServiceProvider();

        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<SiteDbContext>().Database.EnsureCreated();
    }

    /// <summary>The root service provider the initializer creates its own scope from.</summary>
    public ServiceProvider Services { get; }

    public void Dispose()
    {
        Services.Dispose();
        _connection.Dispose();
    }
}
