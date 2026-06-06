namespace ProManagerOnline.Site.Contracts;

/// <summary>
/// The documentation administration API consumed by the Blazor admin components. The same interface
/// has a server implementation (calling the application handlers in process) and a WebAssembly
/// implementation (calling the JSON API over HTTP), so Interactive Auto components work either way.
/// Operations throw <see cref="DocsAdminException"/> on failure.
/// </summary>
public interface IDocsAdminApi
{
    /// <summary>Lists the published products documentation can be attached to.</summary>
    Task<IReadOnlyList<ProductOption>> GetPublishedProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists a product's articles — drafts and published — for the admin list.</summary>
    Task<IReadOnlyList<DocArticleRow>> GetArticlesAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>Loads a single article for editing, or <see langword="null"/> when it does not exist.</summary>
    Task<DocArticleEdit?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new draft article and returns its identifier.</summary>
    Task<Guid> CreateArticleAsync(CreateDocArticleRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing article's content and position.</summary>
    Task UpdateArticleAsync(Guid id, UpdateDocArticleRequest request, CancellationToken cancellationToken = default);

    /// <summary>Publishes or unpublishes an article.</summary>
    Task SetArticleStatusAsync(Guid id, bool publish, CancellationToken cancellationToken = default);

    /// <summary>Uploads a screenshot for a product's documentation and returns its embeddable URL.</summary>
    Task<string> UploadScreenshotAsync(
        Guid productId, string fileName, Stream content, CancellationToken cancellationToken = default);
}
