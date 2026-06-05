using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Application.Tests.Documentation;

/// <summary>Shared builders for the documentation use-case tests.</summary>
internal static class DocTestData
{
    /// <summary>
    /// Creates and publishes a quote-based product. Quote-based products publish without
    /// plans, keeping documentation tests free of unrelated pricing setup.
    /// </summary>
    /// <param name="slug">The product's slug.</param>
    /// <param name="name">The product's display name.</param>
    /// <returns>A published <see cref="Product"/>.</returns>
    public static Product PublishedProduct(string slug = "workflows", string name = "Workflows.AI")
    {
        var product = Product.CreateDraft(Slug.Create(slug), name, "Business management", "Summary.");
        product.MakeQuoteBased();
        product.Publish();
        return product;
    }

    /// <summary>Creates a published documentation article for the given product.</summary>
    /// <param name="productId">The owning product.</param>
    /// <param name="slug">The article slug.</param>
    /// <param name="title">The article title.</param>
    /// <param name="section">The grouping section.</param>
    /// <param name="position">The order within the section.</param>
    /// <param name="body">The Markdown body.</param>
    /// <returns>A published <see cref="DocArticle"/>.</returns>
    public static DocArticle PublishedArticle(
        ProductId productId, string slug, string title, string section, int position, string body = "Body.")
    {
        var article = DocArticle.CreateDraft(productId, Slug.Create(slug), title, section, body, position);
        article.Publish();
        return article;
    }

    /// <summary>Creates a draft (unpublished) documentation article for the given product.</summary>
    /// <param name="productId">The owning product.</param>
    /// <param name="slug">The article slug.</param>
    /// <param name="title">The article title.</param>
    /// <param name="section">The grouping section.</param>
    /// <param name="position">The order within the section.</param>
    /// <returns>A draft <see cref="DocArticle"/>.</returns>
    public static DocArticle DraftArticle(
        ProductId productId, string slug, string title, string section, int position)
        => DocArticle.CreateDraft(productId, Slug.Create(slug), title, section, "Body.", position);
}
