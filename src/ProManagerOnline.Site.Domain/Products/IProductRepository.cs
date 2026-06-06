using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Domain.Products;

/// <summary>
/// Persistence abstraction for the <see cref="Product"/> aggregate. Defined in the domain
/// and implemented in the infrastructure layer; consumed by application use cases.
/// <para>
/// Follows a unit-of-work model: <see cref="AddAsync"/> and <see cref="RemoveAsync"/> stage
/// changes, and mutations made to a product loaded by <see cref="GetByIdAsync"/> or
/// <see cref="GetBySlugAsync"/> are committed together by <see cref="SaveChangesAsync"/>.
/// </para>
/// </summary>
public interface IProductRepository
{
    /// <summary>Finds a product by its identifier.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The product, or <see langword="null"/> if no product has that id.</returns>
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default);

    /// <summary>Finds a product by its URL-safe slug.</summary>
    /// <param name="slug">The product slug.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The product, or <see langword="null"/> if no product has that slug.</returns>
    Task<Product?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default);

    /// <summary>Determines whether any product already uses the given slug.</summary>
    /// <param name="slug">The slug to check.</param>
    /// <param name="excludeId">A product to ignore in the check, used when editing that product's own slug.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if another product already uses the slug; otherwise <see langword="false"/>.</returns>
    Task<bool> SlugExistsAsync(Slug slug, ProductId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>Lists all published products.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The published products, in no particular order.</returns>
    Task<IReadOnlyList<Product>> ListPublishedAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists every product, including drafts, for administration.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>All products, in no particular order.</returns>
    Task<IReadOnlyList<Product>> ListAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Stages a new product for insertion; commit it with <see cref="SaveChangesAsync"/>.</summary>
    /// <param name="product">The product to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>Stages a product for removal; commit it with <see cref="SaveChangesAsync"/>.</summary>
    /// <param name="product">The product to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task RemoveAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists every pending change — a newly added or removed product, or mutations made to a
    /// product loaded by <see cref="GetByIdAsync"/> or <see cref="GetBySlugAsync"/> — to the store.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
