namespace ProManagerOnline.Site.Contracts;

/// <summary>
/// The product administration API consumed by the Blazor admin components. The same interface has
/// a server implementation (calling the application handlers in process) and a WebAssembly
/// implementation (calling the JSON API over HTTP), so Interactive Auto components work either way.
/// Operations throw <see cref="ProductAdminException"/> on failure.
/// </summary>
public interface IProductAdminApi
{
    /// <summary>Lists every product (including drafts).</summary>
    Task<IReadOnlyList<ProductListItem>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a product's full detail, or <see langword="null"/> if it does not exist.</summary>
    Task<ProductDetail?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates a draft product and returns its new identifier.</summary>
    Task<Guid> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates a product's details and slug.</summary>
    Task UpdateDetailsAsync(Guid id, UpdateProductDetailsRequest request, CancellationToken cancellationToken = default);

    /// <summary>Switches a product's pricing kind.</summary>
    Task SetPricingAsync(Guid id, SetProductPricingRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a product and its plans.</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Publishes a product.</summary>
    Task PublishAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns a product to draft.</summary>
    Task UnpublishAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Adds a plan (tier) to a product.</summary>
    Task AddPlanAsync(Guid id, AddPlanRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates one of a product's plans.</summary>
    Task UpdatePlanAsync(Guid id, Guid planId, UpdatePlanRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes a plan from a product.</summary>
    Task RemovePlanAsync(Guid id, Guid planId, CancellationToken cancellationToken = default);

    /// <summary>Marks one of a product's plans as the featured tier.</summary>
    Task FeaturePlanAsync(Guid id, Guid planId, CancellationToken cancellationToken = default);
}
