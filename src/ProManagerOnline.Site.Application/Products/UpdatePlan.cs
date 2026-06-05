using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Input for updating a product's plan (tier).</summary>
/// <param name="ProductId">The owning product.</param>
/// <param name="PlanId">The plan to update.</param>
/// <param name="Name">The new plan name.</param>
/// <param name="Description">The new description of who the plan suits.</param>
/// <param name="Amount">The new recurring price amount.</param>
/// <param name="BillingPeriod">How often the price is billed.</param>
/// <param name="Features">The plan's feature lines; blank entries are ignored.</param>
public sealed record UpdatePlanCommand(
    Guid ProductId,
    Guid PlanId,
    string Name,
    string Description,
    decimal Amount,
    BillingPeriod BillingPeriod,
    IReadOnlyList<string> Features);

/// <summary>Updates a plan on a subscription product.</summary>
/// <param name="products">The product repository.</param>
public sealed class UpdatePlanHandler(IProductRepository products)
{
    /// <summary>Applies the plan changes.</summary>
    /// <param name="command">The plan changes.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no product has the given id.</exception>
    /// <exception cref="DomainException">Thrown when the product has no such plan, or the plan name is blank.</exception>
    public async Task Handle(UpdatePlanCommand command, CancellationToken cancellationToken = default)
    {
        var product = await products.GetRequiredAsync(new ProductId(command.ProductId), cancellationToken);
        product.UpdatePlan(
            new PlanId(command.PlanId),
            command.Name,
            command.Description,
            Money.Create(command.Amount, Currency.Usd),
            command.BillingPeriod,
            command.Features);
        await products.UpdateAsync(product, cancellationToken);
    }
}
