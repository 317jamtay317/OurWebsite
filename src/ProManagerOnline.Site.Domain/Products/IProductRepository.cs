using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Domain.Products;

/// <summary>
/// Persistence abstraction for the <see cref="Product"/> aggregate. Defined in the domain
/// and implemented in the infrastructure layer; consumed by application use cases.
/// </summary>
public interface IProductRepository
{
    /// <summary>Finds a product by its identifier.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The product, or <see langword="null"/> if no product has that id.</returns>
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default);

    /// <summary>Determines whether any product already uses the given slug.</summary>
    /// <param name="slug">The slug to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if a product already uses the slug; otherwise <see langword="false"/>.</returns>
    Task<bool> SlugExistsAsync(Slug slug, CancellationToken cancellationToken = default);

    /// <summary>Lists all published products.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The published products, in no particular order.</returns>
    Task<IReadOnlyList<Product>> ListPublishedAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds a new product to the store.</summary>
    /// <param name="product">The product to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
}
