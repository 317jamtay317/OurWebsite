namespace ProManagerOnline.Site.Application.Products;

/// <summary>A read model summarising a product for listings on the public site and admin.</summary>
/// <param name="Id">The product's identifier.</param>
/// <param name="Slug">The product's URL-safe slug.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Category">The product's category label.</param>
/// <param name="Summary">The product's one-line summary.</param>
public sealed record ProductSummaryDto(Guid Id, string Slug, string Name, string Category, string Summary);
