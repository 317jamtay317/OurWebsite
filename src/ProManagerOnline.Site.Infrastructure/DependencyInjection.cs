using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Infrastructure.Persistence;

namespace ProManagerOnline.Site.Infrastructure;

/// <summary>Registers the infrastructure layer's services in the dependency-injection container.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the database context (SQL Server) and the repository implementations.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SiteDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IDocArticleRepository, DocArticleRepository>();

        return services;
    }
}
