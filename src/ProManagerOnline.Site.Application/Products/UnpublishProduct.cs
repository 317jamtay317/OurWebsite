using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Returns a product to draft, removing it from the public site.</summary>
/// <param name="products">The product repository.</param>
public sealed class UnpublishProductHandler(IProductRepository products)
{
    /// <summary>Unpublishes the product with the given id.</summary>
    /// <param name="id">The product to unpublish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no product has the given id.</exception>
    public async Task Handle(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await products.GetRequiredAsync(new ProductId(id), cancellationToken);
        product.Unpublish();
        await products.SaveChangesAsync(cancellationToken);
    }
}
