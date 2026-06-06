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
    public async Task<Product?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        => await context.Products.FirstOrDefaultAsync(product => product.Slug == slug, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> SlugExistsAsync(Slug slug, ProductId? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = context.Products.Where(product => product.Slug == slug);

        if (excludeId is { } id)
        {
            query = query.Where(product => product.Id != id);
        }

        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> ListPublishedAsync(CancellationToken cancellationToken = default)
        => await context.Products
            .Where(product => product.Status == ProductStatus.Published)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> ListAllAsync(CancellationToken cancellationToken = default)
        => await context.Products.ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Add(product);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Remove(product);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
