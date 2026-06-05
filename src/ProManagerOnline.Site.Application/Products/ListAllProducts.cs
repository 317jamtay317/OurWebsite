using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>
/// Returns every product, including drafts, as admin read models for the catalogue screen.
/// </summary>
/// <param name="products">The product repository.</param>
public sealed class ListAllProductsHandler(IProductRepository products)
{
    /// <summary>Lists all products for the admin catalogue.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>All products as admin read models, drafts included.</returns>
    public async Task<IReadOnlyList<AdminProductDto>> Handle(CancellationToken cancellationToken = default)
    {
        var all = await products.ListAllAsync(cancellationToken);

        return all
            .Select(product => new AdminProductDto(
                product.Id.Value,
                product.Slug.Value,
                product.Name,
                product.Category,
                product.Status.ToString(),
                product.PricingKind.ToString(),
                product.Plans.Count))
            .ToList();
    }
}
