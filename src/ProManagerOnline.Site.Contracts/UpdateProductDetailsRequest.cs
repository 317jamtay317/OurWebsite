namespace ProManagerOnline.Site.Contracts;

/// <summary>Request to update a product's display details and slug.</summary>
/// <param name="Slug">The desired URL-safe slug.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Category">The product's category label.</param>
/// <param name="Summary">The product's one-line summary.</param>
public sealed record UpdateProductDetailsRequest(string Slug, string Name, string Category, string Summary);
