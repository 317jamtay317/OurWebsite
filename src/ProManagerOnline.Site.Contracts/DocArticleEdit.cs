namespace ProManagerOnline.Site.Contracts;

/// <summary>Every field the documentation editor form needs for an existing article.</summary>
/// <param name="Id">The article's identifier.</param>
/// <param name="ProductId">The product the article documents.</param>
/// <param name="Slug">The article's URL-safe slug; fixed once created.</param>
/// <param name="Title">The article's title.</param>
/// <param name="Section">The section the article is grouped under.</param>
/// <param name="Body">The article body, authored in Markdown.</param>
/// <param name="Position">The article's order within its section.</param>
/// <param name="Published">Whether the article is published (otherwise it is a draft).</param>
public sealed record DocArticleEdit(
    Guid Id, Guid ProductId, string Slug, string Title, string Section, string Body, int Position, bool Published);
