using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Publishes a product so it appears on the public site.</summary>
/// <param name="products">The product repository.</param>
public sealed class PublishProductHandler(IProductRepository products)
{
    /// <summary>Publishes the product with the given id.</summary>
    /// <param name="id">The product to publish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no product has the given id.</exception>
    /// <exception cref="DomainException">Thrown when the product cannot be published (for example a tiered product with no plans).</exception>
    public async Task Handle(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await products.GetRequiredAsync(new ProductId(id), cancellationToken);
        product.Publish();
        await products.SaveChangesAsync(cancellationToken);
    }
}
