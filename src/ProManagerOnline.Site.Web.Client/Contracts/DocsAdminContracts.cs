namespace ProManagerOnline.Site.Web.Client.Contracts;

/// <summary>A published product the author can attach documentation to.</summary>
/// <param name="Id">The product's identifier.</param>
/// <param name="Name">The product's display name.</param>
public sealed record ProductOption(Guid Id, string Name);

/// <summary>
/// One article as a row in the admin list, where drafts and published articles are shown together.
/// </summary>
/// <param name="Id">The article's identifier.</param>
/// <param name="Slug">The article's URL-safe slug.</param>
/// <param name="Title">The article's title.</param>
/// <param name="Section">The section the article is grouped under.</param>
/// <param name="Position">The article's order within its section.</param>
/// <param name="Published">Whether the article is published (otherwise it is a draft).</param>
public sealed record DocArticleRow(
    Guid Id, string Slug, string Title, string Section, int Position, bool Published);

/// <summary>Every field the editor form needs for an existing article.</summary>
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

/// <summary>Payload to create a new draft article.</summary>
/// <param name="ProductId">The product the article documents.</param>
/// <param name="Slug">The desired URL-safe slug, unique within the product.</param>
/// <param name="Title">The article's title.</param>
/// <param name="Section">The section the article is grouped under.</param>
/// <param name="Body">The article body, authored in Markdown.</param>
/// <param name="Position">The article's order within its section.</param>
public sealed record CreateDocArticleRequest(
    Guid ProductId, string Slug, string Title, string Section, string Body, int Position);

/// <summary>Payload to update an article's content and position. The slug is fixed once created.</summary>
/// <param name="Title">The new title.</param>
/// <param name="Section">The new section.</param>
/// <param name="Body">The new Markdown body.</param>
/// <param name="Position">The new order within the section.</param>
public sealed record UpdateDocArticleRequest(string Title, string Section, string Body, int Position);

/// <summary>Payload to publish or unpublish an article.</summary>
/// <param name="Publish"><see langword="true"/> to publish; <see langword="false"/> to unpublish.</param>
public sealed record SetStatusRequest(bool Publish);

/// <summary>Response returned when an article is created.</summary>
/// <param name="Id">The new article's identifier.</param>
public sealed record CreatedResponse(Guid Id);

/// <summary>Response returned when a screenshot is uploaded.</summary>
/// <param name="Url">The site-relative URL of the stored image, ready to embed in Markdown.</param>
public sealed record UploadResponse(string Url);

/// <summary>Error body returned by the admin API on failure.</summary>
/// <param name="Message">A human-readable description of what went wrong.</param>
public sealed record ErrorResponse(string? Message);
