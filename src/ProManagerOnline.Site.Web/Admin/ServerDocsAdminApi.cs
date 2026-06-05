using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Web.Client.Contracts;
using ProManagerOnline.Site.Web.Client.Services;

namespace ProManagerOnline.Site.Web.Admin;

/// <summary>
/// Server-side <see cref="IDocsAdminApi"/> that fulfils the documentation admin in-process by
/// calling the application use cases directly, mapping their results to the client transport
/// contracts. It is used while an <c>InteractiveAuto</c> component renders on the server (before
/// the WebAssembly runtime takes over) and by the JSON API endpoints the browser client calls.
/// Application exceptions are allowed to propagate; the API endpoints translate them to HTTP
/// status codes, and server-rendered components surface their messages.
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
    public async Task<IReadOnlyList<ProductOption>> GetPublishedProductsAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await listProducts.Handle(cancellationToken);
        return products.Select(product => new ProductOption(product.Id, product.Name)).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DocArticleRow>> GetArticlesAsync(
        Guid productId, CancellationToken cancellationToken = default)
    {
        var rows = await listArticles.Handle(productId, cancellationToken);
        return rows
            .Select(row => new DocArticleRow(
                row.Id, row.Slug, row.Title, row.Section, row.Position, row.Status == DocStatus.Published))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<DocArticleEdit?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await getArticle.Handle(id, cancellationToken);
        return dto is null
            ? null
            : new DocArticleEdit(
                dto.Id, dto.ProductId, dto.Slug, dto.Title, dto.Section, dto.Body, dto.Position,
                dto.Status == DocStatus.Published);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateArticleAsync(
        CreateDocArticleRequest request, CancellationToken cancellationToken = default)
    {
        var id = await createArticle.Handle(
            new CreateDocArticleCommand(
                request.ProductId, request.Slug, request.Title, request.Section, request.Body, request.Position),
            cancellationToken);
        return id.Value;
    }

    /// <inheritdoc />
    public Task UpdateArticleAsync(
        Guid id, UpdateDocArticleRequest request, CancellationToken cancellationToken = default)
        => updateArticle.Handle(
            new UpdateDocArticleCommand(id, request.Title, request.Section, request.Body, request.Position),
            cancellationToken);

    /// <inheritdoc />
    public Task SetArticleStatusAsync(Guid id, bool publish, CancellationToken cancellationToken = default)
        => setStatus.Handle(new SetDocArticleStatusCommand(id, publish), cancellationToken);

    /// <inheritdoc />
    public Task<string> UploadScreenshotAsync(
        Guid productId, string fileName, Stream content, CancellationToken cancellationToken = default)
        => media.SaveScreenshotAsync(productId, fileName, content, cancellationToken);
}
