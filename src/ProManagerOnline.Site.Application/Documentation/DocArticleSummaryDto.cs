namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>A read model describing one article as a link in the documentation sidebar.</summary>
/// <param name="Id">The article's identifier.</param>
/// <param name="Slug">The article's URL-safe slug, used as the page name in its link.</param>
/// <param name="Title">The article's title.</param>
/// <param name="Section">The section the article is grouped under.</param>
/// <param name="Position">The article's order within its section.</param>
public sealed record DocArticleSummaryDto(Guid Id, string Slug, string Title, string Section, int Position);
