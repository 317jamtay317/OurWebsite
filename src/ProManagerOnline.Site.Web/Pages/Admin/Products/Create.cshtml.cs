using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Web.Pages.Admin.Products;

/// <summary>Admin page for creating a new draft product.</summary>
public class CreateModel(CreateProductHandler createProduct) : PageModel
{
    /// <summary>The product details entered in the form.</summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>Renders the empty create form.</summary>
    public void OnGet()
    {
    }

    /// <summary>Creates the product and redirects to its editor.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (Input.PricingKind == PricingKind.Fixed && Input.FixedPriceAmount is null)
        {
            ModelState.AddModelError("Input.FixedPriceAmount", "Enter the one-time price for a fixed-price product.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var command = new CreateProductCommand(
                Input.Slug!, Input.Name!, Input.Category!, Input.Summary!, Input.PricingKind, Input.FixedPriceAmount);
            var id = await createProduct.Handle(command, cancellationToken);
            return RedirectToPage("Edit", new { id = id.Value });
        }
        catch (ConflictException exception)
        {
            ModelState.AddModelError("Input.Slug", exception.Message);
        }
        catch (DomainException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }

        return Page();
    }

    /// <summary>The create form fields.</summary>
    public sealed class InputModel
    {
        /// <summary>The URL-safe slug used in the product's public links.</summary>
        [Required]
        [RegularExpression(
            "^[a-z0-9]+(?:-[a-z0-9]+)*$",
            ErrorMessage = "Use lowercase letters, digits and single hyphens, e.g. 'air-compliance'.")]
        public string? Slug { get; set; }

        /// <summary>The product's display name.</summary>
        [Required]
        public string? Name { get; set; }

        /// <summary>The product's category label.</summary>
        [Required]
        public string? Category { get; set; }

        /// <summary>A one-line summary, also used as the product's description.</summary>
        [Required]
        public string? Summary { get; set; }

        /// <summary>How the product is priced.</summary>
        public PricingKind PricingKind { get; set; } = PricingKind.Tiered;

        /// <summary>The one-time price, used only for fixed-price products.</summary>
        [Range(0, 1_000_000, ErrorMessage = "Enter a price of 0 or more.")]
        public decimal? FixedPriceAmount { get; set; }
    }
}
