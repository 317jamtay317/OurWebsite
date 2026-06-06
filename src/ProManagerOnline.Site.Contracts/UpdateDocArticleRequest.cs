namespace ProManagerOnline.Site.Contracts;

/// <summary>Payload to update a documentation article's content and position. The slug is fixed once created.</summary>
/// <param name="Title">The new title.</param>
/// <param name="Section">The new section.</param>
/// <param name="Body">The new Markdown body.</param>
/// <param name="Position">The new order within the section.</param>
public sealed record UpdateDocArticleRequest(string Title, string Section, string Body, int Position);
