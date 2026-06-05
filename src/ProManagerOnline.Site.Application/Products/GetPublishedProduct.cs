using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>
/// Loads the public page detail of a single published product, addressed by its slug.
/// Drafts, unknown slugs and malformed slugs resolve to <see langword="null"/> so the public
/// site can render a "not found" response.
/// </summary>
/// <param name="products">The product repository.</param>
public sealed class GetPublishedProductHandler(IProductRepository products)
{
    /// <summary>Loads the published product's public page model.</summary>
    /// <param name="slug">The product slug taken from the public URL.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The product's public page model, or <see langword="null"/> when <paramref name="slug"/> is
    /// not a valid slug, no product uses it, or the product is not published.
    /// </returns>
    public async Task<ProductPageDto?> Handle(string slug, CancellationToken cancellationToken = default)
    {
        if (!Slug.TryCreate(slug, out var parsedSlug))
        {
            return null;
        }

        var product = await products.GetBySlugAsync(parsedSlug, cancellationToken);
        if (product is null || product.Status != ProductStatus.Published)
        {
            return null;
        }

        var plans = product.Plans
            .Select(plan => new ProductPagePlanDto(
                plan.Name,
                plan.Description,
                plan.Price.Amount,
                plan.Price.Currency.ToString(),
                plan.BillingPeriod,
                plan.IsFeatured,
                plan.Features))
            .ToList();

        return new ProductPageDto(
            product.Slug.Value,
            product.Name,
            product.Category,
            product.Summary,
            product.PricingKind,
            product.FixedPrice?.Amount,
            plans);
    }
}
