namespace ProManagerOnline.Site.Domain.Products;

/// <summary>How a <see cref="Product"/> is priced.</summary>
public enum PricingKind
{
    /// <summary>Priced with one or more subscription <see cref="Plan"/> tiers.</summary>
    Tiered,

    /// <summary>Priced as a single one-time fixed amount (see <see cref="Product.FixedPrice"/>).</summary>
    Fixed,

    /// <summary>No fixed price; customers request a quote.</summary>
    Quote,
}
