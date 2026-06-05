using Microsoft.AspNetCore.Mvc.RazorPages;
using ProManagerOnline.Site.Application.Documentation;

namespace ProManagerOnline.Site.Web.Pages.Docs;

/// <summary>The documentation landing page at <c>/docs</c>: lists the products that have documentation.</summary>
/// <param name="listDocumentedProducts">The query that lists documented products.</param>
public sealed class IndexModel(ListDocumentedProductsHandler listDocumentedProducts) : PageModel
{
    /// <summary>The published products that have documentation.</summary>
    public IReadOnlyList<DocumentedProductDto> Products { get; private set; } = [];

    /// <summary>Loads the documented products for rendering.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Products = await listDocumentedProducts.Handle(cancellationToken);
}
