using Microsoft.Extensions.DependencyInjection;

namespace ProManagerOnline.Site.Application;

/// <summary>Registers the application layer's services in the dependency-injection container.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers every use-case handler in this assembly (types whose name ends in
    /// <c>Handler</c>) as a scoped service.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var handlers = typeof(DependencyInjection).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && type.Name.EndsWith("Handler", StringComparison.Ordinal));

        foreach (var handler in handlers)
        {
            services.AddScoped(handler);
        }

        return services;
    }
}
