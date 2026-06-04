using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Contracts;

/// <summary>Request to add a subscription plan (tier) to a product.</summary>
/// <param name="Name">The tier name.</param>
/// <param name="Description">A short description of who the tier suits.</param>
/// <param name="Amount">The recurring price amount.</param>
/// <param name="BillingPeriod">How often the price is billed.</param>
/// <param name="Features">The tier's feature lines; blank entries are ignored.</param>
public sealed record AddPlanRequest(
    string Name,
    string Description,
    decimal Amount,
    BillingPeriod BillingPeriod,
    IReadOnlyList<string> Features);
