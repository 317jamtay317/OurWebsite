using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Contracts;

/// <summary>The full detail of a product, used by the admin editor.</summary>
/// <param name="Id">The product's identifier.</param>
/// <param name="Slug">The product's URL-safe slug.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Category">The product's category label.</param>
/// <param name="Summary">The product's one-line summary (also used as its description).</param>
/// <param name="Status">Whether the product is a draft or published.</param>
/// <param name="PricingKind">How the product is priced.</param>
/// <param name="FixedPriceAmount">The one-time price when fixed-price; otherwise <see langword="null"/>.</param>
/// <param name="Plans">The product's subscription plans, empty unless it is tiered.</param>
public sealed record ProductDetail(
    Guid Id,
    string Slug,
    string Name,
    string Category,
    string Summary,
    ProductStatus Status,
    PricingKind PricingKind,
    decimal? FixedPriceAmount,
    IReadOnlyList<PlanDetail> Plans);
