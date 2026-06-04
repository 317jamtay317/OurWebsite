using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Lists every product (including drafts) for the admin product list.</summary>
/// <param name="products">The product repository.</param>
public sealed class ListProductsHandler(IProductRepository products)
{
    /// <summary>Lists all products as admin summary read models.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>All products, in no particular order.</returns>
    public async Task<IReadOnlyList<ProductListItem>> Handle(CancellationToken cancellationToken = default)
    {
        var all = await products.ListAllAsync(cancellationToken);

        return all
            .Select(product => new ProductListItem(
                product.Id.Value,
                product.Slug.Value,
                product.Name,
                product.Category,
                product.Status,
                product.PricingKind,
                product.Plans.Count,
                product.FixedPrice?.Amount))
            .ToList();
    }
}
