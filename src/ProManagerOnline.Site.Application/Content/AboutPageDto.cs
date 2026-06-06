namespace ProManagerOnline.Site.Application.Content;

/// <summary>The public read model for the About page, used to render the <c>/about</c> page.</summary>
/// <param name="Title">The page heading.</param>
/// <param name="Body">The page body, authored in Markdown.</param>
public sealed record AboutPageDto(string Title, string Body);
