using ProManagerOnline.Site.Domain.Documentation;

namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>
/// A read model carrying every field the authoring editor needs to populate its form for an
/// existing article.
/// </summary>
/// <param name="Id">The article's identifier.</param>
/// <param name="ProductId">The product the article documents — used to scope screenshot uploads.</param>
/// <param name="Slug">The article's URL-safe slug. Fixed once created, so shown read-only when editing.</param>
/// <param name="Title">The article's title.</param>
/// <param name="Section">The section the article is grouped under.</param>
/// <param name="Body">The article body, authored in Markdown.</param>
/// <param name="Position">The article's order within its section.</param>
/// <param name="Status">Whether the article is a draft or published.</param>
public sealed record DocArticleEditDto(
    Guid Id, Guid ProductId, string Slug, string Title, string Section, string Body, int Position, DocStatus Status);
