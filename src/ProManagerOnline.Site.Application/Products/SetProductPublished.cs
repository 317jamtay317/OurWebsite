using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Input for publishing or unpublishing a product.</summary>
/// <param name="ProductId">The product to change.</param>
/// <param name="Published">
/// <see langword="true"/> to publish the product; <see langword="false"/> to return it to draft.
/// </param>
public sealed record SetProductPublishedCommand(Guid ProductId, bool Published);

/// <summary>
/// Publishes or unpublishes a product, applying the domain's publishing rules.
/// </summary>
/// <param name="products">The product repository.</param>
public sealed class SetProductPublishedHandler(IProductRepository products)
{
    /// <summary>Applies the requested publication state to the product.</summary>
    /// <param name="command">The product and desired publication state.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no product has the given id.</exception>
    /// <exception cref="Domain.Exceptions.DomainException">
    /// Thrown when publishing is not allowed (for example, a tiered product with no plans).
    /// </exception>
    public async Task Handle(SetProductPublishedCommand command, CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(new ProductId(command.ProductId), cancellationToken)
            ?? throw new NotFoundException($"No product with id {command.ProductId} exists.");

        if (command.Published)
        {
            product.Publish();
        }
        else
        {
            product.Unpublish();
        }

        await products.SaveChangesAsync(cancellationToken);
    }
}
