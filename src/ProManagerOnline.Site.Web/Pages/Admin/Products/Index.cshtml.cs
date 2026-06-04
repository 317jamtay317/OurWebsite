using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Domain.Exceptions;

namespace ProManagerOnline.Site.Web.Pages.Admin.Products;

/// <summary>Admin product list: every product with actions to edit, publish/unpublish or delete.</summary>
public class IndexModel(
    ListProductsHandler listProducts,
    PublishProductHandler publishProduct,
    UnpublishProductHandler unpublishProduct,
    DeleteProductHandler deleteProduct) : PageModel
{
    /// <summary>The products to display.</summary>
    public IReadOnlyList<ProductListItemDto> Products { get; private set; } = [];

    /// <summary>A status or error message shown once after an action.</summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>Loads all products for the list.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    public async Task OnGetAsync(CancellationToken cancellationToken)
        => Products = await listProducts.Handle(cancellationToken);

    /// <summary>Publishes a product and returns to the list.</summary>
    /// <param name="id">The product to publish.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    public async Task<IActionResult> OnPostPublishAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await publishProduct.Handle(id, cancellationToken);
            StatusMessage = "Product published.";
        }
        catch (Exception exception) when (exception is DomainException or NotFoundException)
        {
            StatusMessage = $"Could not publish: {exception.Message}";
        }

        return RedirectToPage();
    }

    /// <summary>Unpublishes a product and returns to the list.</summary>
    /// <param name="id">The product to unpublish.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    public async Task<IActionResult> OnPostUnpublishAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await unpublishProduct.Handle(id, cancellationToken);
            StatusMessage = "Product moved to draft.";
        }
        catch (NotFoundException exception)
        {
            StatusMessage = exception.Message;
        }

        return RedirectToPage();
    }

    /// <summary>Deletes a product and returns to the list.</summary>
    /// <param name="id">The product to delete.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await deleteProduct.Handle(id, cancellationToken);
            StatusMessage = "Product deleted.";
        }
        catch (NotFoundException exception)
        {
            StatusMessage = exception.Message;
        }

        return RedirectToPage();
    }
}
