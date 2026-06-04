namespace ProManagerOnline.Site.Domain.Products;

/// <summary>
/// Strongly-typed identifier for a <see cref="Plan"/> within a <see cref="Product"/>.
/// </summary>
public readonly record struct PlanId
{
    /// <summary>Wraps an existing identifier value.</summary>
    /// <param name="value">The underlying GUID.</param>
    public PlanId(Guid value) => Value = value;

    /// <summary>The underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, unique plan identifier.</summary>
    /// <returns>A <see cref="PlanId"/> wrapping a freshly generated GUID.</returns>
    public static PlanId New() => new(Guid.NewGuid());
}
