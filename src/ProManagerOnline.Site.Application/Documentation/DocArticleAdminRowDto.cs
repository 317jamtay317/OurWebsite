using ProManagerOnline.Site.Domain.Documentation;

namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>
/// A read model describing one article as a row in the authoring admin's article list, where
/// drafts and published articles are shown together.
/// </summary>
/// <param name="Id">The article's identifier.</param>
/// <param name="Slug">The article's URL-safe slug.</param>
/// <param name="Title">The article's title.</param>
/// <param name="Section">The section the article is grouped under.</param>
/// <param name="Position">The article's order within its section.</param>
/// <param name="Status">Whether the article is a draft or published.</param>
public sealed record DocArticleAdminRowDto(
    Guid Id, string Slug, string Title, string Section, int Position, DocStatus Status);
