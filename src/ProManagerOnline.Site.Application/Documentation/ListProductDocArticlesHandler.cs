using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>
/// Lists every documentation article of a product — drafts and published — as admin rows for the
/// authoring admin, ordered by section then position.
/// </summary>
/// <param name="articles">The documentation-article repository.</param>
public sealed class ListProductDocArticlesHandler(IDocArticleRepository articles)
{
    /// <summary>Lists the product's articles for the admin.</summary>
    /// <param name="productId">The product's identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The product's articles as admin rows, ordered by section then position; empty when the
    /// product has no articles.
    /// </returns>
    public async Task<IReadOnlyList<DocArticleAdminRowDto>> Handle(
        Guid productId, CancellationToken cancellationToken = default)
    {
        var all = await articles.ListByProductAsync(new ProductId(productId), cancellationToken);

        return all
            .Select(article => new DocArticleAdminRowDto(
                article.Id.Value, article.Slug.Value, article.Title, article.Section, article.Position, article.Status))
            .ToList();
    }
}
