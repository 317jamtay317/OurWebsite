using Microsoft.EntityFrameworkCore;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Infrastructure.Persistence;

/// <summary>Seeds the catalogue with the initial products when the database is empty.</summary>
public static class SiteDbSeeder
{
    /// <summary>Adds the initial published products if the catalogue is currently empty.</summary>
    /// <param name="context">The site database context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public static async Task SeedAsync(SiteDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var workflows = Product.CreateDraft(
            Slug.Create("workflows"),
            "Workflows.AI",
            "Business management",
            "An all-in-one platform that brings customers, quotes, invoicing, inventory and accounting together — with workflow automation for the routine work.");
        workflows.AddPlan(
            "Solo", "For an owner-operator getting organised.", Money.Create(29m, Currency.Usd), BillingPeriod.Monthly,
            ["1 user", "Customers & CRM", "Quotes & invoicing", "Email support"]);
        var team = workflows.AddPlan(
            "Team", "For small contractors and crews.", Money.Create(79m, Currency.Usd), BillingPeriod.Monthly,
            ["Up to 5 users", "Everything in Solo", "Inventory management", "Workflow automation", "Priority email support"]);
        workflows.AddPlan(
            "Business", "For teams that have outgrown five seats.", Money.Create(149m, Currency.Usd), BillingPeriod.Monthly,
            ["Unlimited users", "Everything in Team", "Accounting & reporting", "API access", "Phone & email support"]);
        workflows.FeaturePlan(team);
        workflows.Publish();

        var airCompliance = Product.CreateDraft(
            Slug.Create("air-compliance"),
            "Air Compliance Record Keeping",
            "Environmental compliance",
            "Purpose-built for hot-mix asphalt plants: log daily production, track permit limits automatically, and generate the air-emissions reports your state agency expects.");
        airCompliance.MakeQuoteBased();
        airCompliance.Publish();

        context.Products.Add(workflows);
        context.Products.Add(airCompliance);
        await context.SaveChangesAsync(cancellationToken);
    }
}
