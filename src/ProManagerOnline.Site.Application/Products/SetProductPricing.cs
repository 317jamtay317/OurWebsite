using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Input for switching a product's pricing kind.</summary>
/// <param name="Id">The product to change.</param>
/// <param name="Kind">The pricing kind to switch to.</param>
/// <param name="FixedPriceAmount">The one-time price, required when switching to fixed pricing.</param>
public sealed record SetProductPricingCommand(Guid Id, PricingKind Kind, decimal? FixedPriceAmount = null);

/// <summary>Switches a product between subscription, fixed-price and quote-based pricing.</summary>
/// <param name="products">The product repository.</param>
public sealed class SetProductPricingHandler(IProductRepository products)
{
    /// <summary>Applies the pricing change.</summary>
    /// <param name="command">The pricing change.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no product has the given id.</exception>
    /// <exception cref="DomainException">Thrown when switching to fixed pricing without a price.</exception>
    public async Task Handle(SetProductPricingCommand command, CancellationToken cancellationToken = default)
    {
        var product = await products.GetRequiredAsync(new ProductId(command.Id), cancellationToken);

        switch (command.Kind)
        {
            case PricingKind.Tiered:
                product.MakeTiered();
                break;
            case PricingKind.Fixed:
                if (command.FixedPriceAmount is null)
                {
                    throw new DomainException("A fixed-price product needs a price.");
                }

                product.MakeFixedPrice(Money.Create(command.FixedPriceAmount.Value, Currency.Usd));
                break;
            case PricingKind.Quote:
                product.MakeQuoteBased();
                break;
        }

        await products.UpdateAsync(product, cancellationToken);
    }
}
