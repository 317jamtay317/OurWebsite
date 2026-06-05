using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Contracts;

/// <summary>A product summary for the admin product list (all statuses).</summary>
/// <param name="Id">The product's identifier.</param>
/// <param name="Slug">The product's URL-safe slug.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Category">The product's category label.</param>
/// <param name="Status">Whether the product is a draft or published.</param>
/// <param name="PricingKind">How the product is priced.</param>
/// <param name="PlanCount">The number of subscription plans the product has.</param>
/// <param name="FixedPriceAmount">The one-time price when fixed-price; otherwise <see langword="null"/>.</param>
public sealed record ProductListItem(
    Guid Id,
    string Slug,
    string Name,
    string Category,
    ProductStatus Status,
    PricingKind PricingKind,
    int PlanCount,
    decimal? FixedPriceAmount);
