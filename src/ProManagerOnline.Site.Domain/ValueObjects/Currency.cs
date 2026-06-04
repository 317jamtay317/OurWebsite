namespace ProManagerOnline.Site.Domain.ValueObjects;

/// <summary>
/// The currencies a product can be priced in. The catalogue currently prices
/// everything in US dollars; additional currencies are added here. Values use the
/// ISO 4217 numeric code so they remain stable if reordered.
/// </summary>
public enum Currency
{
    /// <summary>United States dollar (USD).</summary>
    Usd = 840,
}
