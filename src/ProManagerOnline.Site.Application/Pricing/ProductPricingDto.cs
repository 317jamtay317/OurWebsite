namespace ProManagerOnline.Site.Application.Pricing;

/// <summary>
/// A published product as shown on the public pricing page, including its pricing shape and,
/// for tiered products, its plans. Unlike the admin read models this never exposes drafts.
/// </summary>
/// <param name="Slug">The product's URL-safe slug, used to preselect it on the contact form.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Category">The product's category label.</param>
/// <param name="Summary">The product's one-line summary.</param>
/// <param name="PricingKind">The pricing shape: "Tiered", "Fixed" or "Quote".</param>
/// <param name="FixedPrice">The one-time price when the product is fixed-price; otherwise <see langword="null"/>.</param>
/// <param name="Currency">The currency of <paramref name="FixedPrice"/>; otherwise <see langword="null"/>.</param>
/// <param name="Plans">The subscription plans, in display order. Empty unless the product is tiered.</param>
public sealed record ProductPricingDto(
    string Slug,
    string Name,
    string Category,
    string Summary,
    string PricingKind,
    decimal? FixedPrice,
    string? Currency,
    IReadOnlyList<PricingPlanDto> Plans);
