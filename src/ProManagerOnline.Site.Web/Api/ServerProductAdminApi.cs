using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Domain.Exceptions;

namespace ProManagerOnline.Site.Web.Api;

/// <summary>
/// Server-side <see cref="IProductAdminApi"/> that calls the application handlers in process. Used
/// when Interactive Auto components render on the server, and by the JSON API. Application/domain
/// exceptions are translated to <see cref="ProductAdminException"/> so callers handle them uniformly.
/// </summary>
public sealed class ServerProductAdminApi(
    ListProductsHandler listProducts,
    GetProductHandler getProduct,
    CreateProductHandler createProduct,
    UpdateProductDetailsHandler updateDetails,
    SetProductPricingHandler setPricing,
    DeleteProductHandler deleteProduct,
    PublishProductHandler publishProduct,
    UnpublishProductHandler unpublishProduct,
    AddPlanHandler addPlan,
    UpdatePlanHandler updatePlan,
    RemovePlanHandler removePlan,
    FeaturePlanHandler featurePlan) : IProductAdminApi
{
    /// <inheritdoc />
    public Task<IReadOnlyList<ProductListItem>> ListAsync(CancellationToken cancellationToken = default)
        => Guard(() => listProducts.Handle(cancellationToken));

    /// <inheritdoc />
    public Task<ProductDetail?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Guard(() => getProduct.Handle(id, cancellationToken));

    /// <inheritdoc />
    public Task<Guid> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        => Guard(async () => (await createProduct.Handle(
            new CreateProductCommand(
                request.Slug, request.Name, request.Category, request.Summary,
                request.PricingKind, request.FixedPriceAmount),
            cancellationToken)).Value);

    /// <inheritdoc />
    public Task UpdateDetailsAsync(Guid id, UpdateProductDetailsRequest request, CancellationToken cancellationToken = default)
        => Guard(() => updateDetails.Handle(
            new UpdateProductDetailsCommand(id, request.Slug, request.Name, request.Category, request.Summary),
            cancellationToken));

    /// <inheritdoc />
    public Task SetPricingAsync(Guid id, SetProductPricingRequest request, CancellationToken cancellationToken = default)
        => Guard(() => setPricing.Handle(
            new SetProductPricingCommand(id, request.Kind, request.FixedPriceAmount), cancellationToken));

    /// <inheritdoc />
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => Guard(() => deleteProduct.Handle(id, cancellationToken));

    /// <inheritdoc />
    public Task PublishAsync(Guid id, CancellationToken cancellationToken = default)
        => Guard(() => publishProduct.Handle(id, cancellationToken));

    /// <inheritdoc />
    public Task UnpublishAsync(Guid id, CancellationToken cancellationToken = default)
        => Guard(() => unpublishProduct.Handle(id, cancellationToken));

    /// <inheritdoc />
    public Task AddPlanAsync(Guid id, AddPlanRequest request, CancellationToken cancellationToken = default)
        => Guard(() => addPlan.Handle(
            new AddPlanCommand(id, request.Name, request.Description, request.Amount, request.BillingPeriod, request.Features),
            cancellationToken));

    /// <inheritdoc />
    public Task UpdatePlanAsync(Guid id, Guid planId, UpdatePlanRequest request, CancellationToken cancellationToken = default)
        => Guard(() => updatePlan.Handle(
            new UpdatePlanCommand(id, planId, request.Name, request.Description, request.Amount, request.BillingPeriod, request.Features),
            cancellationToken));

    /// <inheritdoc />
    public Task RemovePlanAsync(Guid id, Guid planId, CancellationToken cancellationToken = default)
        => Guard(() => removePlan.Handle(new RemovePlanCommand(id, planId), cancellationToken));

    /// <inheritdoc />
    public Task FeaturePlanAsync(Guid id, Guid planId, CancellationToken cancellationToken = default)
        => Guard(() => featurePlan.Handle(new FeaturePlanCommand(id, planId), cancellationToken));

    private static async Task Guard(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception) when (exception is DomainException or ConflictException or NotFoundException)
        {
            throw Map(exception);
        }
    }

    private static async Task<T> Guard<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception exception) when (exception is DomainException or ConflictException or NotFoundException)
        {
            throw Map(exception);
        }
    }

    private static ProductAdminException Map(Exception exception) => exception switch
    {
        NotFoundException => new ProductAdminException(ProductAdminError.NotFound, exception.Message),
        ConflictException => new ProductAdminException(ProductAdminError.Conflict, exception.Message),
        _ => new ProductAdminException(ProductAdminError.Invalid, exception.Message),
    };
}
