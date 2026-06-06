using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Input for updating a product's display details and slug.</summary>
/// <param name="Id">The product to update.</param>
/// <param name="Slug">The desired URL-safe slug.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Category">The product's category label.</param>
/// <param name="Summary">The product's one-line summary.</param>
public sealed record UpdateProductDetailsCommand(Guid Id, string Slug, string Name, string Category, string Summary);

/// <summary>Updates a product's details, changing its slug only when it differs and is unused.</summary>
/// <param name="products">The product repository.</param>
public sealed class UpdateProductDetailsHandler(IProductRepository products)
{
    /// <summary>Applies the detail changes.</summary>
    /// <param name="command">The new details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no product has the given id.</exception>
    /// <exception cref="ConflictException">Thrown when the requested slug is used by another product.</exception>
    public async Task Handle(UpdateProductDetailsCommand command, CancellationToken cancellationToken = default)
    {
        var product = await products.GetRequiredAsync(new ProductId(command.Id), cancellationToken);
        var slug = Slug.Create(command.Slug);

        if (slug != product.Slug)
        {
            if (await products.SlugExistsAsync(slug, product.Id, cancellationToken))
            {
                throw new ConflictException($"A product with the slug '{slug.Value}' already exists.");
            }

            product.ChangeSlug(slug);
        }

        product.UpdateDetails(command.Name, command.Category, command.Summary);
        await products.SaveChangesAsync(cancellationToken);
    }
}
