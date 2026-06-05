using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Convenience helpers shared by the product use cases.</summary>
internal static class ProductRepositoryExtensions
{
    /// <summary>Loads a product by id, throwing <see cref="NotFoundException"/> when it does not exist.</summary>
    public static async Task<Product> GetRequiredAsync(
        this IProductRepository products, ProductId id, CancellationToken cancellationToken)
        => await products.GetByIdAsync(id, cancellationToken)
           ?? throw new NotFoundException($"No product was found with id {id.Value}.");
}
