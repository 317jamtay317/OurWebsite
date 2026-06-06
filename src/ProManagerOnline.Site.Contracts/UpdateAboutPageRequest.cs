namespace ProManagerOnline.Site.Contracts;

/// <summary>Payload to save new content for the About page.</summary>
/// <param name="Title">The new heading.</param>
/// <param name="Body">The new Markdown body.</param>
public sealed record UpdateAboutPageRequest(string Title, string Body);
