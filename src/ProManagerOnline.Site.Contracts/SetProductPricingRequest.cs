using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Contracts;

/// <summary>Request to switch a product's pricing kind.</summary>
/// <param name="Kind">The pricing kind to switch to.</param>
/// <param name="FixedPriceAmount">The one-time price, required when switching to fixed pricing.</param>
public sealed record SetProductPricingRequest(PricingKind Kind, decimal? FixedPriceAmount);
