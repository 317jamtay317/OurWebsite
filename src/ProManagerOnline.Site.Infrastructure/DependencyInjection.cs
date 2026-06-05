using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Application.Administration;
using ProManagerOnline.Site.Application.Email;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Infrastructure.Email;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Infrastructure.Persistence;

namespace ProManagerOnline.Site.Infrastructure;

/// <summary>Registers the infrastructure layer's services in the dependency-injection container.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the database context (SQL Server), the repository implementations, and the
    /// admin account and email services. ASP.NET Core Identity itself is registered by the
    /// web layer, which owns the cookie/sign-in concerns.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SiteDbContext>(options => options
            .UseSqlServer(connectionString)
            // The product aggregate loads plans and their features together. For a small
            // catalogue a single query is fine, so silence the split-query advisory warning.
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.MultipleCollectionIncludeWarning)));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IAdminAccountService, AdminAccountService>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();

        return services;
    }
}
