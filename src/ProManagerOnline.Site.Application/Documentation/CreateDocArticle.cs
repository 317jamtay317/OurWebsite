using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>Input for creating a new draft documentation article.</summary>
/// <param name="ProductId">The product the article documents.</param>
/// <param name="Slug">The desired URL-safe slug, for example <c>getting-started</c>; unique within the product.</param>
/// <param name="Title">The article's title.</param>
/// <param name="Section">The section the article is grouped under, for example "Getting started".</param>
/// <param name="Body">The article body, authored in Markdown.</param>
/// <param name="Position">The article's order within its section; must not be negative.</param>
public sealed record CreateDocArticleCommand(
    Guid ProductId, string Slug, string Title, string Section, string Body, int Position);

/// <summary>
/// Creates a new draft documentation article, ensuring the requested slug is not already used by
/// another article of the same product.
/// </summary>
/// <param name="articles">The documentation-article repository.</param>
public sealed class CreateDocArticleHandler(IDocArticleRepository articles)
{
    /// <summary>Creates the article and returns its new identifier.</summary>
    /// <param name="command">The article details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identifier of the newly created article.</returns>
    /// <exception cref="ConflictException">Thrown when the product already has an article with the slug.</exception>
    public async Task<DocArticleId> Handle(
        CreateDocArticleCommand command, CancellationToken cancellationToken = default)
    {
        var productId = new ProductId(command.ProductId);
        var slug = Slug.Create(command.Slug);

        if (await articles.SlugExistsAsync(productId, slug, cancellationToken))
        {
            throw new ConflictException(
                $"A documentation article with the slug '{slug.Value}' already exists for this product.");
        }

        var article = DocArticle.CreateDraft(
            productId, slug, command.Title, command.Section, command.Body, command.Position);

        await articles.AddAsync(article, cancellationToken);
        await articles.SaveChangesAsync(cancellationToken);

        return article.Id;
    }
}
