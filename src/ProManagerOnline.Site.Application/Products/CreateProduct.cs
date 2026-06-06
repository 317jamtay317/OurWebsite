using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Products;

/// <summary>Input for creating a new draft product.</summary>
/// <param name="Slug">The desired URL-safe slug, for example <c>workflows</c>.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Category">The product's category label.</param>
/// <param name="Summary">A one-line summary of the product.</param>
/// <param name="PricingKind">How the product is priced. Defaults to subscription (tiered).</param>
/// <param name="FixedPriceAmount">The one-time price, required when <paramref name="PricingKind"/> is fixed.</param>
public sealed record CreateProductCommand(
    string Slug,
    string Name,
    string Category,
    string Summary,
    PricingKind PricingKind = PricingKind.Tiered,
    decimal? FixedPriceAmount = null);

/// <summary>
/// Creates a new draft product with the requested pricing kind, ensuring the slug is unused.
/// </summary>
/// <param name="products">The product repository.</param>
public sealed class CreateProductHandler(IProductRepository products)
{
    /// <summary>Creates the product and returns its new identifier.</summary>
    /// <param name="command">The product details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identifier of the newly created product.</returns>
    /// <exception cref="ConflictException">Thrown when the slug is already used by another product.</exception>
    /// <exception cref="DomainException">Thrown when a fixed-price product is requested without a price.</exception>
    public async Task<ProductId> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var slug = Slug.Create(command.Slug);

        if (await products.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"A product with the slug '{slug.Value}' already exists.");
        }

        var product = Product.CreateDraft(slug, command.Name, command.Category, command.Summary);
        ApplyPricing(product, command);
        await products.AddAsync(product, cancellationToken);
        await products.SaveChangesAsync(cancellationToken);

        return product.Id;
    }

    private static void ApplyPricing(Product product, CreateProductCommand command)
    {
        switch (command.PricingKind)
        {
            case PricingKind.Fixed:
                if (command.FixedPriceAmount is null)
                {
                    throw new DomainException("A fixed-price product needs a price.");
                }

                product.MakeFixedPrice(Money.Create(command.FixedPriceAmount.Value, Currency.Usd));
                break;
            case PricingKind.Quote:
                product.MakeQuoteBased();
                break;
            case PricingKind.Tiered:
            default:
                break;
        }
    }
}
