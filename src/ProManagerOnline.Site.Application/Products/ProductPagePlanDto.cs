using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>A single subscription plan (tier) shown on a public product page.</summary>
/// <param name="Name">The plan's display name, for example "Team".</param>
/// <param name="Description">A short description of who the plan suits.</param>
/// <param name="Amount">The recurring price amount.</param>
/// <param name="Currency">The ISO currency code the amount is expressed in.</param>
/// <param name="BillingPeriod">How often the price is billed.</param>
/// <param name="IsFeatured">Whether this plan is highlighted as the recommended tier.</param>
/// <param name="Features">The plan's feature lines, in display order.</param>
public sealed record ProductPagePlanDto(
    string Name,
    string Description,
    decimal Amount,
    string Currency,
    BillingPeriod BillingPeriod,
    bool IsFeatured,
    IReadOnlyList<string> Features);
