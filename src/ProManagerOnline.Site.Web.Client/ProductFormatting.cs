using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Domain.Products;

namespace ProManagerOnline.Site.Web.Client;

/// <summary>Display helpers shared by the product admin components.</summary>
public static class ProductFormatting
{
    /// <summary>Formats a money amount, e.g. <c>$1,499.99</c>.</summary>
    /// <param name="amount">The amount.</param>
    /// <returns>The formatted price.</returns>
    public static string Price(decimal amount) => amount.ToString("$#,##0.##");

    /// <summary>Returns the billing-period suffix, e.g. <c>/mo</c> or <c>/yr</c>.</summary>
    /// <param name="period">The billing period.</param>
    /// <returns>The suffix.</returns>
    public static string Billing(BillingPeriod period) => period == BillingPeriod.Monthly ? "/mo" : "/yr";

    /// <summary>Describes a product's pricing for the admin list.</summary>
    /// <param name="product">The product summary.</param>
    /// <returns>A short pricing description.</returns>
    public static string PricingSummary(ProductListItem product) => product.PricingKind switch
    {
        PricingKind.Tiered => $"Subscription · {product.PlanCount} {(product.PlanCount == 1 ? "tier" : "tiers")}",
        PricingKind.Fixed => $"Fixed price · {Price(product.FixedPriceAmount ?? 0m)} one-time",
        _ => "Quote-based",
    };
}
