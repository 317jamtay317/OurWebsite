using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Domain.Exceptions;

namespace ProManagerOnline.Site.Web.Api;

/// <summary>
/// Server-side <see cref="IDocsAdminApi"/> that calls the application handlers in process. Used when
/// Interactive Auto components render on the server, and by the JSON API. Application/domain
/// exceptions are translated to <see cref="DocsAdminException"/> so callers handle them uniformly.
/// </summary>
/// <param name="listProducts">Lists published products for the picker.</param>
/// <param name="listArticles">Lists a product's articles (all statuses).</param>
/// <param name="getArticle">Loads a single article for editing.</param>
/// <param name="createArticle">Creates a draft article.</param>
/// <param name="updateArticle">Edits an article's content and position.</param>
/// <param name="setStatus">Publishes or unpublishes an article.</param>
/// <param name="media">Stores uploaded screenshots.</param>
public sealed class ServerDocsAdminApi(
    ListPublishedProductsHandler listProducts,
    ListProductDocArticlesHandler listArticles,
    GetDocArticleForEditHandler getArticle,
    CreateDocArticleHandler createArticle,
    UpdateDocArticleHandler updateArticle,
    SetDocArticleStatusHandler setStatus,
    IDocMediaStorage media) : IDocsAdminApi
{
    /// <inheritdoc />
    public Task<IReadOnlyList<ProductOption>> GetPublishedProductsAsync(CancellationToken cancellationToken = default)
        => Guard(async () =>
        {
            var products = await listProducts.Handle(cancellationToken);
            return (IReadOnlyList<ProductOption>)products
                .Select(product => new ProductOption(product.Id, product.Name))
                .ToList();
        });

    /// <inheritdoc />
    public Task<IReadOnlyList<DocArticleRow>> GetArticlesAsync(Guid productId, CancellationToken cancellationToken = default)
        => Guard(() => listArticles.Handle(productId, cancellationToken));

    /// <inheritdoc />
    public Task<DocArticleEdit?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default)
        => Guard(() => getArticle.Handle(id, cancellationToken));

    /// <inheritdoc />
    public Task<Guid> CreateArticleAsync(CreateDocArticleRequest request, CancellationToken cancellationToken = default)
        => Guard(async () => (await createArticle.Handle(
            new CreateDocArticleCommand(
                request.ProductId, request.Slug, request.Title, request.Section, request.Body, request.Position),
            cancellationToken)).Value);

    /// <inheritdoc />
    public Task UpdateArticleAsync(Guid id, UpdateDocArticleRequest request, CancellationToken cancellationToken = default)
        => Guard(() => updateArticle.Handle(
            new UpdateDocArticleCommand(id, request.Title, request.Section, request.Body, request.Position),
            cancellationToken));

    /// <inheritdoc />
    public Task SetArticleStatusAsync(Guid id, bool publish, CancellationToken cancellationToken = default)
        => Guard(() => setStatus.Handle(new SetDocArticleStatusCommand(id, publish), cancellationToken));

    /// <inheritdoc />
    public Task<string> UploadScreenshotAsync(
        Guid productId, string fileName, Stream content, CancellationToken cancellationToken = default)
        => Guard(() => media.SaveScreenshotAsync(productId, fileName, content, cancellationToken));

    private static async Task Guard(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception) when (exception is DomainException or ConflictException or NotFoundException)
        {
            throw Map(exception);
        }
    }

    private static async Task<T> Guard<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception exception) when (exception is DomainException or ConflictException or NotFoundException)
        {
            throw Map(exception);
        }
    }

    private static DocsAdminException Map(Exception exception) => exception switch
    {
        NotFoundException => new DocsAdminException(DocsAdminError.NotFound, exception.Message),
        ConflictException => new DocsAdminException(DocsAdminError.Conflict, exception.Message),
        _ => new DocsAdminException(DocsAdminError.Invalid, exception.Message),
    };
}
