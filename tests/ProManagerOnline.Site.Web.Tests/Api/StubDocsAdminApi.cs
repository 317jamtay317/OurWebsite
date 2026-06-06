using ProManagerOnline.Site.Contracts;

namespace ProManagerOnline.Site.Web.Tests.Api;

/// <summary>
/// A do-nothing <see cref="IDocsAdminApi"/> so the documentation admin endpoints can be mapped and
/// their routes inspected without a database or the application handlers.
/// </summary>
internal sealed class StubDocsAdminApi : IDocsAdminApi
{
    public Task<IReadOnlyList<ProductOption>> GetPublishedProductsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ProductOption>>([]);

    public Task<IReadOnlyList<DocArticleRow>> GetArticlesAsync(Guid productId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DocArticleRow>>([]);

    public Task<DocArticleEdit?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<DocArticleEdit?>(null);

    public Task<Guid> CreateArticleAsync(CreateDocArticleRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Guid.Empty);

    public Task UpdateArticleAsync(Guid id, UpdateDocArticleRequest request, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task SetArticleStatusAsync(Guid id, bool publish, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<string> UploadScreenshotAsync(
        Guid productId, string fileName, Stream content, CancellationToken cancellationToken = default)
        => Task.FromResult(string.Empty);
}
