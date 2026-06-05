using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Application.Administration;
using ProManagerOnline.Site.Application.Email;
using ProManagerOnline.Site.Application.Security;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Infrastructure.Email;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Infrastructure.Persistence;
using ProManagerOnline.Site.Infrastructure.Security;

namespace ProManagerOnline.Site.Infrastructure;

/// <summary>Registers the infrastructure layer's services in the dependency-injection container.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the database context (SQL Server), the repository implementations, the admin
    /// account service, an email sender, and the reCAPTCHA validator. The email sender is
    /// SMTP-backed when an SMTP host is configured, and a development logging sender otherwise.
    /// ASP.NET Core Identity itself is registered by the web layer, which owns the sign-in concerns.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The application configuration (connection string and options).</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the <c>SiteDatabase</c> connection string is missing.</exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SiteDatabase")
            ?? throw new InvalidOperationException("The 'SiteDatabase' connection string is not configured.");

        services.AddDbContext<SiteDbContext>(options => options
            .UseSqlServer(connectionString)
            // The product aggregate loads plans and their features together. For a small
            // catalogue a single query is fine, so silence the split-query advisory warning.
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.MultipleCollectionIncludeWarning)));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IDocArticleRepository, DocArticleRepository>();
        services.AddScoped<IAdminAccountService, AdminAccountService>();

        AddEmailSender(services, configuration);
        AddCaptchaValidator(services, configuration);

        return services;
    }

    private static void AddEmailSender(IServiceCollection services, IConfiguration configuration)
    {
        var smtpSection = configuration.GetSection(SmtpOptions.SectionName);

        // Fall back to the development logging sender until an SMTP host is configured, so the
        // contact form runs end to end locally (the enquiry appears in the server log).
        if (string.IsNullOrWhiteSpace(smtpSection[nameof(SmtpOptions.Host)]))
        {
            services.AddScoped<IEmailSender, LoggingEmailSender>();
            return;
        }

        services.Configure<SmtpOptions>(smtpSection);
        services.AddScoped<IEmailSender, SmtpEmailSender>();
    }

    private static void AddCaptchaValidator(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RecaptchaOptions>(configuration.GetSection(RecaptchaOptions.SectionName));
        services.AddHttpClient<ICaptchaValidator, GoogleRecaptchaValidator>();
    }
}
