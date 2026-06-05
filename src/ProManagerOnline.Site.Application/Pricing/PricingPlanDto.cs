namespace ProManagerOnline.Site.Application.Pricing;

/// <summary>A single subscription tier of a product, as shown on the public pricing page.</summary>
/// <param name="Name">The plan's display name, for example "Team".</param>
/// <param name="Description">A short description of who the plan suits.</param>
/// <param name="Amount">The recurring price amount.</param>
/// <param name="Currency">The currency the <paramref name="Amount"/> is expressed in, for example "Usd".</param>
/// <param name="BillingPeriod">How often the price is billed: "Monthly" or "Annual".</param>
/// <param name="IsFeatured">Whether this plan is highlighted as the recommended tier.</param>
/// <param name="Features">The plan's feature lines, in display order.</param>
public sealed record PricingPlanDto(
    string Name,
    string Description,
    decimal Amount,
    string Currency,
    string BillingPeriod,
    bool IsFeatured,
    IReadOnlyList<string> Features);
