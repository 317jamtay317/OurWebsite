using Microsoft.AspNetCore.Mvc.RazorPages;
using ProManagerOnline.Site.Application.Products;

namespace ProManagerOnline.Site.Web.Pages;

/// <summary>Home page: lists the published products read from the database.</summary>
public class IndexModel(ListPublishedProductsHandler listPublishedProducts) : PageModel
{
    /// <summary>The published products to display.</summary>
    public IReadOnlyList<ProductSummaryDto> Products { get; private set; } = [];

    /// <summary>Loads the published products for rendering.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Products = await listPublishedProducts.Handle(cancellationToken);
}
