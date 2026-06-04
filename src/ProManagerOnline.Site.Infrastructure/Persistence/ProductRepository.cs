using Microsoft.EntityFrameworkCore;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Infrastructure.Persistence;

/// <summary>EF Core implementation of <see cref="IProductRepository"/>.</summary>
/// <param name="context">The site database context.</param>
public sealed class ProductRepository(SiteDbContext context) : IProductRepository
{
    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
        => await context.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> SlugExistsAsync(Slug slug, CancellationToken cancellationToken = default)
        => await context.Products.AnyAsync(product => product.Slug == slug, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> ListPublishedAsync(CancellationToken cancellationToken = default)
        => await context.Products
            .Where(product => product.Status == ProductStatus.Published)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);
    }
}
