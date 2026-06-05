namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>A read model for a single documentation article's content.</summary>
/// <param name="Id">The article's identifier.</param>
/// <param name="Slug">The article's URL-safe slug.</param>
/// <param name="Title">The article's title.</param>
/// <param name="Section">The section the article is grouped under.</param>
/// <param name="Body">
/// The article body, authored in Markdown. Rendering it to HTML is a presentation concern.
/// </param>
public sealed record DocPageDto(Guid Id, string Slug, string Title, string Section, string Body);
