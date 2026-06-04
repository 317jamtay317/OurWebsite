using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Input for creating a new draft product.</summary>
/// <param name="Slug">The desired URL-safe slug, for example <c>workflows</c>.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Category">The product's category label.</param>
/// <param name="Summary">A one-line summary of the product.</param>
public sealed record CreateProductCommand(string Slug, string Name, string Category, string Summary);

/// <summary>
/// Creates a new draft product, ensuring the requested slug is not already in use.
/// </summary>
/// <param name="products">The product repository.</param>
public sealed class CreateProductHandler(IProductRepository products)
{
    /// <summary>Creates the product and returns its new identifier.</summary>
    /// <param name="command">The product details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identifier of the newly created product.</returns>
    /// <exception cref="ConflictException">Thrown when the slug is already used by another product.</exception>
    public async Task<ProductId> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var slug = Slug.Create(command.Slug);

        if (await products.SlugExistsAsync(slug, cancellationToken))
        {
            throw new ConflictException($"A product with the slug '{slug.Value}' already exists.");
        }

        var product = Product.CreateDraft(slug, command.Name, command.Category, command.Summary);
        await products.AddAsync(product, cancellationToken);

        return product.Id;
    }
}
