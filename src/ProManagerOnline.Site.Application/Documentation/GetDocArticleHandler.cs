using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>
/// Returns the content of a single published documentation article that belongs to a published
/// product.
/// </summary>
/// <param name="products">The product repository, used to confirm the product is published.</param>
/// <param name="articles">The documentation-article repository.</param>
public sealed class GetDocArticleHandler(IProductRepository products, IDocArticleRepository articles)
{
    /// <summary>Loads a published article by its product and page name (slug).</summary>
    /// <param name="productId">The product's identifier.</param>
    /// <param name="pageName">The article's slug, taken from the page-name URL segment.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The article's content, or <see langword="null"/> when the product is not published, the
    /// page name is not a valid slug, or no published article has that slug.
    /// </returns>
    public async Task<DocPageDto?> Handle(
        Guid productId, string pageName, CancellationToken cancellationToken = default)
    {
        var id = new ProductId(productId);

        var product = await products.GetByIdAsync(id, cancellationToken);
        if (product is null || product.Status != ProductStatus.Published)
        {
            return null;
        }

        if (!TryParseSlug(pageName, out var slug))
        {
            return null;
        }

        var article = await articles.GetPublishedAsync(id, slug, cancellationToken);

        return article is null
            ? null
            : new DocPageDto(article.Id.Value, article.Slug.Value, article.Title, article.Section, article.Body);
    }

    private static bool TryParseSlug(string value, out Slug slug)
    {
        try
        {
            slug = Slug.Create(value);
            return true;
        }
        catch (DomainException)
        {
            // A page-name URL segment that is not a valid slug simply matches no article.
            slug = null!;
            return false;
        }
    }
}
