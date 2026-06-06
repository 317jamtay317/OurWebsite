using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IProductRepository"/> used as a test double for application
/// handlers, so use cases can be tested without a database.
/// </summary>
internal sealed class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<ProductId, Product> _products = [];

    /// <summary>The number of times <see cref="SaveChangesAsync"/> has been called, for assertions.</summary>
    public int SaveChangesCount { get; private set; }

    public Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.GetValueOrDefault(id));

    public Task<Product?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.Values.FirstOrDefault(product => product.Slug == slug));

    public Task<bool> SlugExistsAsync(Slug slug, ProductId? excludeId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.Values.Any(product =>
            product.Slug == slug && (excludeId is null || product.Id != excludeId.Value)));

    public Task<IReadOnlyList<Product>> ListPublishedAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Product>>(
            _products.Values.Where(product => product.Status == ProductStatus.Published).ToList());

    public Task<IReadOnlyList<Product>> ListAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Product>>(_products.Values.ToList());

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products.Remove(product.Id);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}
