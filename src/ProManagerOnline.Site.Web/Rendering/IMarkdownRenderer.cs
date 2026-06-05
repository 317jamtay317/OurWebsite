using Microsoft.AspNetCore.Html;

namespace ProManagerOnline.Site.Web.Rendering;

/// <summary>Renders Markdown source into HTML for display in a Razor page.</summary>
public interface IMarkdownRenderer
{
    /// <summary>Renders a Markdown string to HTML.</summary>
    /// <param name="markdown">The Markdown source to render.</param>
    /// <returns>The rendered HTML, ready to be written into a page.</returns>
    IHtmlContent Render(string markdown);
}
