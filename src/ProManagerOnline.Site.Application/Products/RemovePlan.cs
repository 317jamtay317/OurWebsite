using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Input for removing a plan from a product.</summary>
/// <param name="ProductId">The owning product.</param>
/// <param name="PlanId">The plan to remove.</param>
public sealed record RemovePlanCommand(Guid ProductId, Guid PlanId);

/// <summary>Removes a plan from a subscription product.</summary>
/// <param name="products">The product repository.</param>
public sealed class RemovePlanHandler(IProductRepository products)
{
    /// <summary>Removes the plan.</summary>
    /// <param name="command">The plan to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no product has the given id.</exception>
    /// <exception cref="DomainException">Thrown when the product has no such plan.</exception>
    public async Task Handle(RemovePlanCommand command, CancellationToken cancellationToken = default)
    {
        var product = await products.GetRequiredAsync(new ProductId(command.ProductId), cancellationToken);
        product.RemovePlan(new PlanId(command.PlanId));
        await products.SaveChangesAsync(cancellationToken);
    }
}
