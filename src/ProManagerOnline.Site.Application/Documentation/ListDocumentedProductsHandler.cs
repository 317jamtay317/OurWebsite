using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>
/// Lists the published products that have at least one published documentation article, for the
/// documentation landing page. Products are ordered by name.
/// </summary>
/// <param name="products">The product repository.</param>
/// <param name="articles">The documentation-article repository.</param>
public sealed class ListDocumentedProductsHandler(IProductRepository products, IDocArticleRepository articles)
{
    /// <summary>Lists the documented, published products.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>One summary per published product that has published documentation.</returns>
    public async Task<IReadOnlyList<DocumentedProductDto>> Handle(CancellationToken cancellationToken = default)
    {
        var documentedIds = (await articles.ListProductIdsWithPublishedDocsAsync(cancellationToken)).ToHashSet();
        if (documentedIds.Count == 0)
        {
            return [];
        }

        var published = await products.ListPublishedAsync(cancellationToken);

        var documented = new List<DocumentedProductDto>();
        foreach (var product in published
                     .Where(product => documentedIds.Contains(product.Id))
                     .OrderBy(product => product.Name, StringComparer.Ordinal))
        {
            var articleCount = (await articles.ListPublishedByProductAsync(product.Id, cancellationToken)).Count;
            documented.Add(new DocumentedProductDto(product.Id.Value, product.Name, product.Summary, articleCount));
        }

        return documented;
    }
}
