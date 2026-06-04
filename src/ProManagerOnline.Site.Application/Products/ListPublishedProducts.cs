using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>
/// Returns the published products for display on the public site, as summary read models.
/// </summary>
/// <param name="products">The product repository.</param>
public sealed class ListPublishedProductsHandler(IProductRepository products)
{
    /// <summary>Lists the published products.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The published products as summary read models.</returns>
    public async Task<IReadOnlyList<ProductSummaryDto>> Handle(CancellationToken cancellationToken = default)
    {
        var published = await products.ListPublishedAsync(cancellationToken);

        return published
            .Select(product => new ProductSummaryDto(
                product.Id.Value, product.Slug.Value, product.Name, product.Category, product.Summary))
            .ToList();
    }
}
