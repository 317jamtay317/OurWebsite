namespace ProManagerOnline.Site.Domain.Products;

/// <summary>How often a subscription <see cref="Plan"/> is billed.</summary>
public enum BillingPeriod
{
    /// <summary>Billed once per month.</summary>
    Monthly,

    /// <summary>Billed once per year.</summary>
    Annual,
}
