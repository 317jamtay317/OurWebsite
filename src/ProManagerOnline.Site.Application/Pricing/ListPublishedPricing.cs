using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Pricing;

/// <summary>
/// Returns the published products with their pricing, for the public pricing page. Products are
/// ordered so priced ones (tiered, then fixed) come before quote-based ones.
/// </summary>
/// <param name="products">The product repository.</param>
public sealed class ListPublishedPricingHandler(IProductRepository products)
{
    /// <summary>Lists the published products and their pricing as read models.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The published products as pricing read models.</returns>
    public async Task<IReadOnlyList<ProductPricingDto>> Handle(CancellationToken cancellationToken = default)
    {
        var published = await products.ListPublishedAsync(cancellationToken);

        return published
            .OrderBy(product => product.PricingKind)
            .ThenBy(product => product.Name, StringComparer.OrdinalIgnoreCase)
            .Select(ToDto)
            .ToList();
    }

    private static ProductPricingDto ToDto(Product product) =>
        new(
            product.Slug.Value,
            product.Name,
            product.Category,
            product.Summary,
            product.PricingKind.ToString(),
            product.FixedPrice?.Amount,
            product.FixedPrice?.Currency.ToString(),
            product.Plans
                .Select(plan => new PricingPlanDto(
                    plan.Name,
                    plan.Description,
                    plan.Price.Amount,
                    plan.Price.Currency.ToString(),
                    plan.BillingPeriod.ToString(),
                    plan.IsFeatured,
                    plan.Features))
                .ToList());
}
