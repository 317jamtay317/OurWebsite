namespace ProManagerOnline.Site.Contracts;

/// <summary>The editable content of the About page, as the admin editor reads and displays it.</summary>
/// <param name="Title">The page heading.</param>
/// <param name="Body">The page body, authored in Markdown.</param>
public sealed record AboutPageContent(string Title, string Body);
