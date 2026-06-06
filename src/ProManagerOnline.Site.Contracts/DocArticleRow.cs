namespace ProManagerOnline.Site.Contracts;

/// <summary>
/// One documentation article as a row in the admin list, where drafts and published articles are
/// shown together.
/// </summary>
/// <param name="Id">The article's identifier.</param>
/// <param name="Slug">The article's URL-safe slug.</param>
/// <param name="Title">The article's title.</param>
/// <param name="Section">The section the article is grouped under.</param>
/// <param name="Position">The article's order within its section.</param>
/// <param name="Published">Whether the article is published (otherwise it is a draft).</param>
public sealed record DocArticleRow(
    Guid Id, string Slug, string Title, string Section, int Position, bool Published);
