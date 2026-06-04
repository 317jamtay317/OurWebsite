namespace ProManagerOnline.Site.Domain.Products;

/// <summary>
/// Strongly-typed identifier for a <see cref="Product"/>. Prevents accidentally mixing
/// identifiers from different aggregates.
/// </summary>
public readonly record struct ProductId
{
    /// <summary>Wraps an existing identifier value.</summary>
    /// <param name="value">The underlying GUID.</param>
    public ProductId(Guid value) => Value = value;

    /// <summary>The underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, unique product identifier.</summary>
    /// <returns>A <see cref="ProductId"/> wrapping a freshly generated GUID.</returns>
    public static ProductId New() => new(Guid.NewGuid());
}
