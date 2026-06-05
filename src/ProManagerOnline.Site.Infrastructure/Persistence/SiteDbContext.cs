using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Infrastructure.Identity;

namespace ProManagerOnline.Site.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the site's product catalogue and documentation, and for
/// the ASP.NET Core Identity admin accounts (the <c>AspNet*</c> tables).
/// </summary>
public sealed class SiteDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>Initialises the context with the given options.</summary>
    /// <param name="options">The context options (database provider, connection and so on).</param>
    public SiteDbContext(DbContextOptions<SiteDbContext> options)
        : base(options)
    {
    }

    /// <summary>The products in the catalogue.</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>The documentation articles.</summary>
    public DbSet<DocArticle> DocArticles => Set<DocArticle>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the Identity schema first, then the catalogue/documentation mappings.
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SiteDbContext).Assembly);
    }
}
