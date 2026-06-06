namespace ProManagerOnline.Site.Contracts;

/// <summary>Payload to create a new draft documentation article.</summary>
/// <param name="ProductId">The product the article documents.</param>
/// <param name="Slug">The desired URL-safe slug, unique within the product.</param>
/// <param name="Title">The article's title.</param>
/// <param name="Section">The section the article is grouped under.</param>
/// <param name="Body">The article body, authored in Markdown.</param>
/// <param name="Position">The article's order within its section.</param>
public sealed record CreateDocArticleRequest(
    Guid ProductId, string Slug, string Title, string Section, string Body, int Position);
