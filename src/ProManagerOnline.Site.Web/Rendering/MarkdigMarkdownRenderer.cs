using Markdig;
using Microsoft.AspNetCore.Html;

namespace ProManagerOnline.Site.Web.Rendering;

/// <summary>
/// Renders Markdown to HTML using Markdig with its advanced extensions (tables, auto-links and
/// the like). Documentation content is authored by the site owner, so it is trusted and rendered
/// without HTML sanitisation; if untrusted authoring is ever added, sanitise the output here.
/// </summary>
public sealed class MarkdigMarkdownRenderer : IMarkdownRenderer
{
    /// <inheritdoc />
    public IHtmlContent Render(string markdown)
        => new HtmlString(Markdown.ToHtml(markdown ?? string.Empty, Pipeline));

    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
}
