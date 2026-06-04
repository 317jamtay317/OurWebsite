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

    public Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.GetValueOrDefault(id));

    public Task<bool> SlugExistsAsync(Slug slug, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.Values.Any(product => product.Slug == slug));

    public Task<IReadOnlyList<Product>> ListPublishedAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Product>>(
            _products.Values.Where(product => product.Status == ProductStatus.Published).ToList());

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products[product.Id] = product;
        return Task.CompletedTask;
    }
}
