namespace ProManagerOnline.Site.Application.Products;

/// <summary>
/// A read model describing a product for the admin catalogue list, including its publication
/// status and pricing shape (which the public <see cref="ProductSummaryDto"/> omits).
/// </summary>
/// <param name="Id">The product's identifier.</param>
/// <param name="Slug">The product's URL-safe slug.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Category">The product's category label.</param>
/// <param name="Status">The publication status (<c>Draft</c> or <c>Published</c>).</param>
/// <param name="PricingKind">The pricing shape (<c>Tiered</c> or <c>Quote</c>).</param>
/// <param name="PlanCount">The number of pricing plans the product has.</param>
public sealed record AdminProductDto(
    Guid Id,
    string Slug,
    string Name,
    string Category,
    string Status,
    string PricingKind,
    int PlanCount);
