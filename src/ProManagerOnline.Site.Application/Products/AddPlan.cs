using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Input for adding a subscription plan (tier) to a product.</summary>
/// <param name="ProductId">The product to add the plan to.</param>
/// <param name="Name">The plan name.</param>
/// <param name="Description">A short description of who the plan suits.</param>
/// <param name="Amount">The recurring price amount.</param>
/// <param name="BillingPeriod">How often the price is billed.</param>
/// <param name="Features">The plan's feature lines; blank entries are ignored.</param>
public sealed record AddPlanCommand(
    Guid ProductId,
    string Name,
    string Description,
    decimal Amount,
    BillingPeriod BillingPeriod,
    IReadOnlyList<string> Features);

/// <summary>Adds a plan to a subscription product.</summary>
/// <param name="products">The product repository.</param>
public sealed class AddPlanHandler(IProductRepository products)
{
    /// <summary>Adds the plan.</summary>
    /// <param name="command">The plan details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no product has the given id.</exception>
    /// <exception cref="DomainException">Thrown when the product is not a subscription product, or the plan name is blank.</exception>
    public async Task Handle(AddPlanCommand command, CancellationToken cancellationToken = default)
    {
        var product = await products.GetRequiredAsync(new ProductId(command.ProductId), cancellationToken);
        product.AddPlan(
            command.Name,
            command.Description,
            Money.Create(command.Amount, Currency.Usd),
            command.BillingPeriod,
            command.Features);
        await products.SaveChangesAsync(cancellationToken);
    }
}
