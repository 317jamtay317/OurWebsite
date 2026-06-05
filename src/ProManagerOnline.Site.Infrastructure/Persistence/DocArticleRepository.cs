using Microsoft.EntityFrameworkCore;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Infrastructure.Persistence;

/// <summary>EF Core implementation of <see cref="IDocArticleRepository"/>.</summary>
/// <param name="context">The site database context.</param>
public sealed class DocArticleRepository(SiteDbContext context) : IDocArticleRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<DocArticle>> ListPublishedByProductAsync(
        ProductId productId, CancellationToken cancellationToken = default)
        => await context.DocArticles
            .Where(article => article.ProductId == productId && article.Status == DocStatus.Published)
            .OrderBy(article => article.Section)
            .ThenBy(article => article.Position)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<DocArticle?> GetPublishedAsync(
        ProductId productId, Slug slug, CancellationToken cancellationToken = default)
        => await context.DocArticles.FirstOrDefaultAsync(
            article => article.ProductId == productId
                       && article.Slug == slug
                       && article.Status == DocStatus.Published,
            cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductId>> ListProductIdsWithPublishedDocsAsync(
        CancellationToken cancellationToken = default)
        => await context.DocArticles
            .Where(article => article.Status == DocStatus.Published)
            .Select(article => article.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<DocArticle?> GetByIdAsync(DocArticleId id, CancellationToken cancellationToken = default)
        => await context.DocArticles.FirstOrDefaultAsync(article => article.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<DocArticle>> ListByProductAsync(
        ProductId productId, CancellationToken cancellationToken = default)
        => await context.DocArticles
            .Where(article => article.ProductId == productId)
            .OrderBy(article => article.Section)
            .ThenBy(article => article.Position)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<bool> SlugExistsAsync(
        ProductId productId, Slug slug, CancellationToken cancellationToken = default)
        => await context.DocArticles.AnyAsync(
            article => article.ProductId == productId && article.Slug == slug, cancellationToken);

    /// <inheritdoc />
    public Task AddAsync(DocArticle article, CancellationToken cancellationToken = default)
    {
        context.DocArticles.Add(article);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
