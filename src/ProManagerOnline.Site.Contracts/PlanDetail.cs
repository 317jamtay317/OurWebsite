using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Contracts;

/// <summary>A single subscription plan (tier) for the admin editor.</summary>
/// <param name="Id">The plan's identifier.</param>
/// <param name="Name">The plan's display name, for example "Pro".</param>
/// <param name="Description">A short description of who the plan suits.</param>
/// <param name="Amount">The recurring price amount.</param>
/// <param name="Currency">The ISO currency code the amount is expressed in.</param>
/// <param name="BillingPeriod">How often the price is billed.</param>
/// <param name="IsFeatured">Whether this plan is highlighted as the recommended tier.</param>
/// <param name="Features">The plan's feature lines, in display order.</param>
public sealed record PlanDetail(
    Guid Id,
    string Name,
    string Description,
    decimal Amount,
    string Currency,
    BillingPeriod BillingPeriod,
    bool IsFeatured,
    IReadOnlyList<string> Features);
