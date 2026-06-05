using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Web.Rendering;

namespace ProManagerOnline.Site.Web.Pages.Docs;

/// <summary>
/// The documentation reader at <c>/docs/{productId}/{pageName?}</c>. It always shows the
/// product's section/article sidebar; when a page name is supplied it renders that article,
/// otherwise it shows an overview of the product's documentation.
/// </summary>
/// <param name="getProductDocs">The query that builds the sidebar navigation.</param>
/// <param name="getDocArticle">The query that loads a single article.</param>
/// <param name="markdown">The Markdown renderer used for the article body.</param>
public sealed class ArticleModel(
    GetProductDocsHandler getProductDocs,
    GetDocArticleHandler getDocArticle,
    IMarkdownRenderer markdown) : PageModel
{
    /// <summary>The product's documentation navigation (the sidebar tree).</summary>
    public DocNavigationDto Navigation { get; private set; } = null!;

    /// <summary>The article being read, or <see langword="null"/> on the product overview.</summary>
    public DocPageDto? Article { get; private set; }

    /// <summary>The current article's body rendered to HTML, or <see langword="null"/> on the overview.</summary>
    public IHtmlContent? ArticleHtml { get; private set; }

    /// <summary>Loads the navigation and, when a page name is supplied, the requested article.</summary>
    /// <param name="productId">The product's GUID, from the route.</param>
    /// <param name="pageName">The article slug, from the route; <see langword="null"/> on the overview.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The page, or a 404 when the product or the requested article cannot be found.</returns>
    public async Task<IActionResult> OnGetAsync(Guid productId, string? pageName, CancellationToken cancellationToken)
    {
        var navigation = await getProductDocs.Handle(productId, cancellationToken);
        if (navigation is null)
        {
            return NotFound();
        }

        Navigation = navigation;

        if (!string.IsNullOrEmpty(pageName))
        {
            var article = await getDocArticle.Handle(productId, pageName, cancellationToken);
            if (article is null)
            {
                return NotFound();
            }

            Article = article;
            ArticleHtml = markdown.Render(article.Body);
        }

        return Page();
    }
}
