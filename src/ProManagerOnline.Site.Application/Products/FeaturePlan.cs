using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Input for marking a plan as the featured (recommended) tier.</summary>
/// <param name="ProductId">The owning product.</param>
/// <param name="PlanId">The plan to feature.</param>
public sealed record FeaturePlanCommand(Guid ProductId, Guid PlanId);

/// <summary>Marks one of a product's plans as the featured tier, clearing the flag from the others.</summary>
/// <param name="products">The product repository.</param>
public sealed class FeaturePlanHandler(IProductRepository products)
{
    /// <summary>Features the plan.</summary>
    /// <param name="command">The plan to feature.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no product has the given id.</exception>
    /// <exception cref="DomainException">Thrown when the product has no such plan.</exception>
    public async Task Handle(FeaturePlanCommand command, CancellationToken cancellationToken = default)
    {
        var product = await products.GetRequiredAsync(new ProductId(command.ProductId), cancellationToken);
        product.FeaturePlan(new PlanId(command.PlanId));
        await products.SaveChangesAsync(cancellationToken);
    }
}
