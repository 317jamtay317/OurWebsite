using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Loads the full detail of a single product for the admin editor.</summary>
/// <param name="products">The product repository.</param>
public sealed class GetProductHandler(IProductRepository products)
{
    /// <summary>Loads the product's detail.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The product detail, or <see langword="null"/> if no product has that id.</returns>
    public async Task<ProductDetailDto?> Handle(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(new ProductId(id), cancellationToken);
        if (product is null)
        {
            return null;
        }

        var plans = product.Plans
            .Select(plan => new PlanDetailDto(
                plan.Id.Value,
                plan.Name,
                plan.Description,
                plan.Price.Amount,
                plan.Price.Currency.ToString(),
                plan.BillingPeriod,
                plan.IsFeatured,
                plan.Features))
            .ToList();

        return new ProductDetailDto(
            product.Id.Value,
            product.Slug.Value,
            product.Name,
            product.Category,
            product.Summary,
            product.Status,
            product.PricingKind,
            product.FixedPrice?.Amount,
            plans);
    }
}
