using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Domain.Documentation;

namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>
/// Loads a single documentation article — draft or published — by its identifier, as the edit
/// read model the authoring editor populates its form from.
/// </summary>
/// <param name="articles">The documentation-article repository.</param>
public sealed class GetDocArticleForEditHandler(IDocArticleRepository articles)
{
    /// <summary>Loads the article for editing.</summary>
    /// <param name="id">The article's identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The article's edit read model, or <see langword="null"/> when no article has that id.</returns>
    public async Task<DocArticleEdit?> Handle(Guid id, CancellationToken cancellationToken = default)
    {
        var article = await articles.GetByIdAsync(new DocArticleId(id), cancellationToken);

        return article is null
            ? null
            : new DocArticleEdit(
                article.Id.Value,
                article.ProductId.Value,
                article.Slug.Value,
                article.Title,
                article.Section,
                article.Body,
                article.Position,
                article.Status == DocStatus.Published);
    }
}
