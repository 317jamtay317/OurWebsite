namespace ProManagerOnline.Site.Domain.Content;

/// <summary>
/// Strongly-typed identifier for the <see cref="AboutPage"/>. The About page is a singleton —
/// there is only ever one — so the identifier has a single well-known value, <see cref="Single"/>.
/// </summary>
public readonly record struct AboutPageId
{
    /// <summary>Wraps an existing identifier value.</summary>
    /// <param name="value">The underlying GUID.</param>
    public AboutPageId(Guid value) => Value = value;

    /// <summary>The one and only identity the About page ever has.</summary>
    public static AboutPageId Single { get; } = new(new Guid("a9f3b1c2-0000-0000-0000-000000000001"));

    /// <summary>The underlying GUID value.</summary>
    public Guid Value { get; }
}
