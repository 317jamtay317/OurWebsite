using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Web.Pages.Admin.Products;

/// <summary>Admin editor for a single product: details, pricing, plans (tiers) and publication.</summary>
public class EditModel(
    GetProductHandler getProduct,
    UpdateProductDetailsHandler updateDetails,
    SetProductPricingHandler setPricing,
    AddPlanHandler addPlan,
    UpdatePlanHandler updatePlan,
    RemovePlanHandler removePlan,
    FeaturePlanHandler featurePlan,
    PublishProductHandler publishProduct,
    UnpublishProductHandler unpublishProduct) : PageModel
{
    /// <summary>The product being edited, loaded for display.</summary>
    public ProductDetailDto Product { get; private set; } = default!;

    /// <summary>The product details form.</summary>
    [BindProperty]
    public DetailsInput Details { get; set; } = new();

    /// <summary>The pricing form.</summary>
    [BindProperty]
    public PricingInput Pricing { get; set; } = new();

    /// <summary>The "add a tier" form.</summary>
    [BindProperty]
    public PlanInput NewPlan { get; set; } = new();

    /// <summary>The per-tier edit form.</summary>
    [BindProperty]
    public EditPlanInput EditPlan { get; set; } = new();

    /// <summary>A status or error message shown once after an action.</summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>Loads the product for editing.</summary>
    /// <param name="id">The product to edit.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await getProduct.Handle(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        Product = product;
        Details = new DetailsInput
        {
            Slug = product.Slug,
            Name = product.Name,
            Category = product.Category,
            Summary = product.Summary,
        };
        Pricing = new PricingInput { Kind = product.PricingKind, FixedPriceAmount = product.FixedPriceAmount };
        return Page();
    }

    /// <summary>Saves the product's details.</summary>
    public Task<IActionResult> OnPostSaveDetailsAsync(Guid id, CancellationToken cancellationToken)
        => RunAsync(id, () => updateDetails.Handle(
            new UpdateProductDetailsCommand(
                id, Details.Slug ?? "", Details.Name ?? "", Details.Category ?? "", Details.Summary ?? ""),
            cancellationToken), "Details saved.");

    /// <summary>Switches the product's pricing kind.</summary>
    public Task<IActionResult> OnPostSetPricingAsync(Guid id, CancellationToken cancellationToken)
        => RunAsync(id, () => setPricing.Handle(
            new SetProductPricingCommand(id, Pricing.Kind, Pricing.FixedPriceAmount), cancellationToken),
            "Pricing updated.");

    /// <summary>Adds a tier to the product.</summary>
    public Task<IActionResult> OnPostAddPlanAsync(Guid id, CancellationToken cancellationToken)
        => RunAsync(id, () => addPlan.Handle(
            new AddPlanCommand(
                id, NewPlan.Name ?? "", NewPlan.Description ?? "", NewPlan.Amount, NewPlan.BillingPeriod,
                SplitFeatures(NewPlan.FeaturesText)),
            cancellationToken), "Tier added.");

    /// <summary>Updates one of the product's tiers.</summary>
    public Task<IActionResult> OnPostUpdatePlanAsync(Guid id, CancellationToken cancellationToken)
        => RunAsync(id, () => updatePlan.Handle(
            new UpdatePlanCommand(
                id, EditPlan.PlanId, EditPlan.Name ?? "", EditPlan.Description ?? "", EditPlan.Amount,
                EditPlan.BillingPeriod, SplitFeatures(EditPlan.FeaturesText)),
            cancellationToken), "Tier updated.");

    /// <summary>Removes a tier from the product.</summary>
    public Task<IActionResult> OnPostRemovePlanAsync(Guid id, Guid planId, CancellationToken cancellationToken)
        => RunAsync(id, () => removePlan.Handle(new RemovePlanCommand(id, planId), cancellationToken), "Tier removed.");

    /// <summary>Marks a tier as the featured (recommended) one.</summary>
    public Task<IActionResult> OnPostFeaturePlanAsync(Guid id, Guid planId, CancellationToken cancellationToken)
        => RunAsync(id, () => featurePlan.Handle(new FeaturePlanCommand(id, planId), cancellationToken), "Featured tier set.");

    /// <summary>Publishes the product.</summary>
    public Task<IActionResult> OnPostPublishAsync(Guid id, CancellationToken cancellationToken)
        => RunAsync(id, () => publishProduct.Handle(id, cancellationToken), "Product published.");

    /// <summary>Unpublishes the product.</summary>
    public Task<IActionResult> OnPostUnpublishAsync(Guid id, CancellationToken cancellationToken)
        => RunAsync(id, () => unpublishProduct.Handle(id, cancellationToken), "Product moved to draft.");

    private async Task<IActionResult> RunAsync(Guid id, Func<Task> action, string successMessage)
    {
        try
        {
            await action();
            StatusMessage = successMessage;
        }
        catch (Exception exception) when (exception is DomainException or ConflictException or NotFoundException)
        {
            StatusMessage = exception.Message;
        }

        return RedirectToPage(new { id });
    }

    private static IReadOnlyList<string> SplitFeatures(string? text)
        => string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0).ToList();

    /// <summary>The product details form fields.</summary>
    public sealed class DetailsInput
    {
        /// <summary>The URL-safe slug.</summary>
        public string? Slug { get; set; }

        /// <summary>The display name.</summary>
        public string? Name { get; set; }

        /// <summary>The category label.</summary>
        public string? Category { get; set; }

        /// <summary>The one-line summary.</summary>
        public string? Summary { get; set; }
    }

    /// <summary>The pricing form fields.</summary>
    public sealed class PricingInput
    {
        /// <summary>The pricing kind to switch to.</summary>
        public PricingKind Kind { get; set; } = PricingKind.Tiered;

        /// <summary>The one-time price, for fixed-price products.</summary>
        public decimal? FixedPriceAmount { get; set; }
    }

    /// <summary>The "add a tier" form fields.</summary>
    public sealed class PlanInput
    {
        /// <summary>The tier name.</summary>
        public string? Name { get; set; }

        /// <summary>A short description of who the tier suits.</summary>
        public string? Description { get; set; }

        /// <summary>The recurring price amount.</summary>
        public decimal Amount { get; set; }

        /// <summary>How often the price is billed.</summary>
        public BillingPeriod BillingPeriod { get; set; } = BillingPeriod.Monthly;

        /// <summary>The tier's features, one per line.</summary>
        public string? FeaturesText { get; set; }
    }

    /// <summary>The per-tier edit form fields.</summary>
    public sealed class EditPlanInput
    {
        /// <summary>The tier being edited.</summary>
        public Guid PlanId { get; set; }

        /// <summary>The tier name.</summary>
        public string? Name { get; set; }

        /// <summary>A short description of who the tier suits.</summary>
        public string? Description { get; set; }

        /// <summary>The recurring price amount.</summary>
        public decimal Amount { get; set; }

        /// <summary>How often the price is billed.</summary>
        public BillingPeriod BillingPeriod { get; set; } = BillingPeriod.Monthly;

        /// <summary>The tier's features, one per line.</summary>
        public string? FeaturesText { get; set; }
    }
}
