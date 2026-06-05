using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Contracts;

/// <summary>Request to update a product's plan (tier).</summary>
/// <param name="Name">The tier name.</param>
/// <param name="Description">A short description of who the tier suits.</param>
/// <param name="Amount">The recurring price amount.</param>
/// <param name="BillingPeriod">How often the price is billed.</param>
/// <param name="Features">The tier's feature lines; blank entries are ignored.</param>
public sealed record UpdatePlanRequest(
    string Name,
    string Description,
    decimal Amount,
    BillingPeriod BillingPeriod,
    IReadOnlyList<string> Features);
