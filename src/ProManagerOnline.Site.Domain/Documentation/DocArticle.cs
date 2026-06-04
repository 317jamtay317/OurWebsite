using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Domain.Documentation;

/// <summary>
/// A documentation article that belongs to a product. The aggregate root for a single
/// help article. Articles are grouped into a named section (for example "Getting started")
/// and ordered within it by <see cref="Position"/>. The owning product is referenced by
/// <see cref="ProductId"/>, following the rule that aggregates reference one another by id.
/// </summary>
public sealed class DocArticle
{
    private DocArticle(
        DocArticleId id,
        ProductId productId,
        Slug slug,
        string title,
        string section,
        string body,
        int position)
    {
        Id = id;
        ProductId = productId;
        Slug = slug;
        Title = title;
        Section = section;
        Body = body;
        Position = position;
        Status = DocStatus.Draft;
    }

    /// <summary>The article's unique identifier.</summary>
    public DocArticleId Id { get; }

    /// <summary>The product this article documents.</summary>
    public ProductId ProductId { get; }

    /// <summary>The URL-safe slug for the article's public link.</summary>
    public Slug Slug { get; private set; }

    /// <summary>The article's title.</summary>
    public string Title { get; private set; }

    /// <summary>The section the article is grouped under, for example "Getting started".</summary>
    public string Section { get; private set; }

    /// <summary>The article body, authored in Markdown.</summary>
    public string Body { get; private set; }

    /// <summary>The article's order within its section, ascending. Never negative.</summary>
    public int Position { get; private set; }

    /// <summary>Whether the article is a draft or published.</summary>
    public DocStatus Status { get; private set; }

    /// <summary>
    /// Creates a new draft documentation article.
    /// </summary>
    /// <param name="productId">The product the article documents.</param>
    /// <param name="slug">The URL-safe slug for the article's link.</param>
    /// <param name="title">The article title; must not be blank.</param>
    /// <param name="section">The grouping section; must not be blank.</param>
    /// <param name="body">The Markdown body; must not be blank.</param>
    /// <param name="position">The order within the section; must not be negative.</param>
    /// <returns>A new <see cref="DocArticle"/> in the <see cref="DocStatus.Draft"/> state.</returns>
    /// <exception cref="DomainException">Thrown when a text field is blank or the position is negative.</exception>
    public static DocArticle CreateDraft(
        ProductId productId,
        Slug slug,
        string title,
        string section,
        string body,
        int position)
    {
        Require(title, nameof(title));
        Require(section, nameof(section));
        Require(body, nameof(body));
        RequireNonNegative(position);

        return new DocArticle(DocArticleId.New(), productId, slug, title.Trim(), section.Trim(), body.Trim(), position);
    }

    /// <summary>Updates the article's title, section and body.</summary>
    /// <param name="title">The new title; must not be blank.</param>
    /// <param name="section">The new section; must not be blank.</param>
    /// <param name="body">The new Markdown body; must not be blank.</param>
    /// <exception cref="DomainException">Thrown when a text field is blank.</exception>
    public void UpdateContent(string title, string section, string body)
    {
        Require(title, nameof(title));
        Require(section, nameof(section));
        Require(body, nameof(body));

        Title = title.Trim();
        Section = section.Trim();
        Body = body.Trim();
    }

    /// <summary>Changes the article's order within its section.</summary>
    /// <param name="position">The new position; must not be negative.</param>
    /// <exception cref="DomainException">Thrown when <paramref name="position"/> is negative.</exception>
    public void Reposition(int position)
    {
        RequireNonNegative(position);
        Position = position;
    }

    /// <summary>Publishes the article so it is visible on the public site.</summary>
    public void Publish() => Status = DocStatus.Published;

    /// <summary>Returns the article to the draft state, hiding it from the public site.</summary>
    public void Unpublish() => Status = DocStatus.Draft;

    private static void Require(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"A documentation article's {field} must not be blank.");
        }
    }

    private static void RequireNonNegative(int position)
    {
        if (position < 0)
        {
            throw new DomainException($"A documentation article's position must not be negative (was {position}).");
        }
    }
}
