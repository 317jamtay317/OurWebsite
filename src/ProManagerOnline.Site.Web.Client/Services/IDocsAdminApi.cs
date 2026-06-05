using ProManagerOnline.Site.Web.Client.Contracts;

namespace ProManagerOnline.Site.Web.Client.Services;

/// <summary>
/// The data gateway the documentation admin components depend on. It has two implementations so
/// the <c>InteractiveAuto</c> components work in both execution contexts: a server-side one that
/// calls the application handlers in-process (used while a component renders on the server), and a
/// WebAssembly one that calls the server's JSON API over HTTP (used once the component runs in the
/// browser). Components depend only on this abstraction.
/// </summary>
public interface IDocsAdminApi
{
    /// <summary>Lists the published products documentation can be attached to.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The published products as picker options.</returns>
    Task<IReadOnlyList<ProductOption>> GetPublishedProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists a product's articles — drafts and published — for the admin list.</summary>
    /// <param name="productId">The product whose articles to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The product's articles as admin rows, ordered by section then position.</returns>
    Task<IReadOnlyList<DocArticleRow>> GetArticlesAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>Loads a single article for editing.</summary>
    /// <param name="id">The article's identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The article's editable fields, or <see langword="null"/> when it does not exist.</returns>
    Task<DocArticleEdit?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new draft article.</summary>
    /// <param name="request">The new article's details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The new article's identifier.</returns>
    /// <exception cref="DocsAdminApiException">Thrown when the slug already exists or the input is invalid.</exception>
    Task<Guid> CreateArticleAsync(CreateDocArticleRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing article's content and position.</summary>
    /// <param name="id">The article to update.</param>
    /// <param name="request">The new content and position.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="DocsAdminApiException">Thrown when the article is missing or the input is invalid.</exception>
    Task UpdateArticleAsync(Guid id, UpdateDocArticleRequest request, CancellationToken cancellationToken = default);

    /// <summary>Publishes or unpublishes an article.</summary>
    /// <param name="id">The article whose status to change.</param>
    /// <param name="publish"><see langword="true"/> to publish; <see langword="false"/> to unpublish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="DocsAdminApiException">Thrown when the article is missing.</exception>
    Task SetArticleStatusAsync(Guid id, bool publish, CancellationToken cancellationToken = default);

    /// <summary>Uploads a screenshot for a product's documentation and returns its embeddable URL.</summary>
    /// <param name="productId">The product the screenshot documents.</param>
    /// <param name="fileName">The uploaded file's name.</param>
    /// <param name="content">The image content.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The site-relative URL of the stored image, ready to embed in Markdown.</returns>
    Task<string> UploadScreenshotAsync(
        Guid productId, string fileName, Stream content, CancellationToken cancellationToken = default);
}
