namespace ProManagerOnline.Site.Domain.Documentation;

/// <summary>Strongly-typed identifier for a <see cref="DocArticle"/>.</summary>
public readonly record struct DocArticleId
{
    /// <summary>Wraps an existing identifier value.</summary>
    /// <param name="value">The underlying GUID.</param>
    public DocArticleId(Guid value) => Value = value;

    /// <summary>The underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, unique documentation-article identifier.</summary>
    /// <returns>A <see cref="DocArticleId"/> wrapping a freshly generated GUID.</returns>
    public static DocArticleId New() => new(Guid.NewGuid());
}
