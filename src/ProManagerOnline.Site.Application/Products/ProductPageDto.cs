using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>
/// The public detail of a single published product, used to render its page on the public
/// site. Carries the product's pricing so the page can show subscription plans, a one-time
/// fixed price, or a quote call-to-action.
/// </summary>
/// <param name="Slug">The product's URL-safe slug.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Category">The product's category label.</param>
/// <param name="Summary">The product's one-line summary.</param>
/// <param name="PricingKind">How the product is priced.</param>
/// <param name="FixedPriceAmount">The one-time price when the product is fixed-price; otherwise <see langword="null"/>.</param>
/// <param name="Plans">The product's subscription plans, in display order; empty unless the product is tiered.</param>
public sealed record ProductPageDto(
    string Slug,
    string Name,
    string Category,
    string Summary,
    PricingKind PricingKind,
    decimal? FixedPriceAmount,
    IReadOnlyList<ProductPagePlanDto> Plans);
