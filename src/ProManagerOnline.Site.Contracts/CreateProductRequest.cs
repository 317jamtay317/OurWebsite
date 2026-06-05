using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Contracts;

/// <summary>Request to create a new draft product.</summary>
/// <param name="Slug">The desired URL-safe slug.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Category">The product's category label.</param>
/// <param name="Summary">A one-line summary of the product.</param>
/// <param name="PricingKind">How the product is priced.</param>
/// <param name="FixedPriceAmount">The one-time price, required when fixed-price.</param>
public sealed record CreateProductRequest(
    string Slug,
    string Name,
    string Category,
    string Summary,
    PricingKind PricingKind,
    decimal? FixedPriceAmount);
