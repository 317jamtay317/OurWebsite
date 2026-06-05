using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Domain.Documentation;

/// <summary>
/// Persistence abstraction for the <see cref="DocArticle"/> aggregate. Defined in the domain
/// and implemented in the infrastructure layer; consumed by application use cases.
/// <para>
/// The <c>Published</c>-suffixed read methods return only <see cref="DocStatus.Published"/>
/// articles, since they serve the public documentation site. The remaining methods support the
/// authoring admin, which works across both drafts and published articles, and follow a
/// unit-of-work model: <see cref="AddAsync"/> stages a new article and mutations to a loaded
/// article are committed by <see cref="SaveChangesAsync"/>.
/// </para>
/// </summary>
public interface IDocArticleRepository
{
    /// <summary>Lists a product's published documentation articles.</summary>
    /// <param name="productId">The product whose articles to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The product's published articles, ordered by <see cref="DocArticle.Section"/> then
    /// <see cref="DocArticle.Position"/>. Empty when the product has no published articles.
    /// </returns>
    Task<IReadOnlyList<DocArticle>> ListPublishedByProductAsync(
        ProductId productId, CancellationToken cancellationToken = default);

    /// <summary>Finds a single published article by its product and slug.</summary>
    /// <param name="productId">The product the article belongs to.</param>
    /// <param name="slug">The article's URL-safe slug, unique within the product.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The published article, or <see langword="null"/> when the product has no published
    /// article with that slug.
    /// </returns>
    Task<DocArticle?> GetPublishedAsync(
        ProductId productId, Slug slug, CancellationToken cancellationToken = default);

    /// <summary>Lists the identifiers of every product that has at least one published article.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The distinct identifiers of products that have published documentation.</returns>
    Task<IReadOnlyList<ProductId>> ListProductIdsWithPublishedDocsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Finds a single article by its identifier, whatever its publication status.</summary>
    /// <param name="id">The article's identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The article, or <see langword="null"/> when no article has that id.</returns>
    /// <remarks>For the authoring admin; returns drafts as well as published articles.</remarks>
    Task<DocArticle?> GetByIdAsync(DocArticleId id, CancellationToken cancellationToken = default);

    /// <summary>Lists every article belonging to a product, whatever its publication status.</summary>
    /// <param name="productId">The product whose articles to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// All of the product's articles — drafts and published — ordered by
    /// <see cref="DocArticle.Section"/> then <see cref="DocArticle.Position"/>. Empty when the
    /// product has no articles.
    /// </returns>
    /// <remarks>For the authoring admin, where drafts must be visible alongside published articles.</remarks>
    Task<IReadOnlyList<DocArticle>> ListByProductAsync(
        ProductId productId, CancellationToken cancellationToken = default);

    /// <summary>Determines whether the product already has an article with the given slug.</summary>
    /// <param name="productId">The product the slug must be unique within.</param>
    /// <param name="slug">The slug to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <see langword="true"/> when the product already has an article (draft or published) using
    /// the slug; otherwise <see langword="false"/>. A slug is unique within its product.
    /// </returns>
    Task<bool> SlugExistsAsync(
        ProductId productId, Slug slug, CancellationToken cancellationToken = default);

    /// <summary>Stages a new article for insertion; commit it with <see cref="SaveChangesAsync"/>.</summary>
    /// <param name="article">The article to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(DocArticle article, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists every pending change — a newly added article or mutations made to an article
    /// loaded by <see cref="GetByIdAsync"/> — to the underlying store.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
