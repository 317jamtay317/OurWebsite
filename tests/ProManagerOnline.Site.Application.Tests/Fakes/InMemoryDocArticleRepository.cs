using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IDocArticleRepository"/> used as a test double for application
/// handlers, so documentation use cases can be tested without a database. The public-reader
/// reads return only published articles; the admin reads return drafts as well, mirroring the
/// production repository.
/// </summary>
internal sealed class InMemoryDocArticleRepository : IDocArticleRepository
{
    /// <summary>Adds an article to the in-memory store, for arranging tests.</summary>
    /// <param name="article">The article to store.</param>
    public void Add(DocArticle article) => _articles.Add(article);

    /// <summary>The number of times <see cref="SaveChangesAsync"/> has been called, for assertions.</summary>
    public int SaveChangesCount { get; private set; }

    public Task<IReadOnlyList<DocArticle>> ListPublishedByProductAsync(
        ProductId productId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DocArticle>>(
            _articles
                .Where(article => article.ProductId == productId && article.Status == DocStatus.Published)
                .OrderBy(article => article.Section, StringComparer.Ordinal)
                .ThenBy(article => article.Position)
                .ToList());

    public Task<DocArticle?> GetPublishedAsync(
        ProductId productId, Slug slug, CancellationToken cancellationToken = default)
        => Task.FromResult(
            _articles.FirstOrDefault(article =>
                article.ProductId == productId
                && article.Slug == slug
                && article.Status == DocStatus.Published));

    public Task<IReadOnlyList<ProductId>> ListProductIdsWithPublishedDocsAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ProductId>>(
            _articles
                .Where(article => article.Status == DocStatus.Published)
                .Select(article => article.ProductId)
                .Distinct()
                .ToList());

    public Task<DocArticle?> GetByIdAsync(DocArticleId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_articles.FirstOrDefault(article => article.Id == id));

    public Task<IReadOnlyList<DocArticle>> ListByProductAsync(
        ProductId productId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DocArticle>>(
            _articles
                .Where(article => article.ProductId == productId)
                .OrderBy(article => article.Section, StringComparer.Ordinal)
                .ThenBy(article => article.Position)
                .ToList());

    public Task<bool> SlugExistsAsync(
        ProductId productId, Slug slug, CancellationToken cancellationToken = default)
        => Task.FromResult(
            _articles.Any(article => article.ProductId == productId && article.Slug == slug));

    public Task AddAsync(DocArticle article, CancellationToken cancellationToken = default)
    {
        _articles.Add(article);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }

    private readonly List<DocArticle> _articles = [];
}
