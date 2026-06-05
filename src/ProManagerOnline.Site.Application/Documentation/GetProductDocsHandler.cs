using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>
/// Builds the documentation navigation (the sidebar tree) for a single published product: its
/// published articles grouped into sections and ordered for display. Sections are ordered by
/// the earliest article position within them, then by name; articles within a section are
/// ordered by position, then by title.
/// </summary>
/// <param name="products">The product repository, used to confirm the product is published.</param>
/// <param name="articles">The documentation-article repository.</param>
public sealed class GetProductDocsHandler(IProductRepository products, IDocArticleRepository articles)
{
    /// <summary>Builds the navigation for the given product's documentation.</summary>
    /// <param name="productId">The product's identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The product's documentation navigation, or <see langword="null"/> when the product does
    /// not exist, is not published, or has no published articles.
    /// </returns>
    public async Task<DocNavigationDto?> Handle(Guid productId, CancellationToken cancellationToken = default)
    {
        var id = new ProductId(productId);

        var product = await products.GetByIdAsync(id, cancellationToken);
        if (product is null || product.Status != ProductStatus.Published)
        {
            return null;
        }

        var published = await articles.ListPublishedByProductAsync(id, cancellationToken);
        if (published.Count == 0)
        {
            return null;
        }

        var sections = published
            .GroupBy(article => article.Section)
            .OrderBy(group => group.Min(article => article.Position))
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new DocSectionDto(
                group.Key,
                [.. group
                    .OrderBy(article => article.Position)
                    .ThenBy(article => article.Title, StringComparer.Ordinal)
                    .Select(article => new DocArticleSummaryDto(
                        article.Id.Value, article.Slug.Value, article.Title, article.Section, article.Position))]))
            .ToList();

        return new DocNavigationDto(product.Id.Value, product.Name, sections);
    }
}
