using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Deletes a product together with its plans.</summary>
/// <param name="products">The product repository.</param>
public sealed class DeleteProductHandler(IProductRepository products)
{
    /// <summary>Deletes the product with the given id.</summary>
    /// <param name="id">The product to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no product has the given id.</exception>
    public async Task Handle(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await products.GetRequiredAsync(new ProductId(id), cancellationToken);
        await products.RemoveAsync(product, cancellationToken);
        await products.SaveChangesAsync(cancellationToken);
    }
}
