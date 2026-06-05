using ProManagerOnline.Site.Application.Pricing;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Pricing;

/// <summary>
/// Behaviour of the <see cref="ListPublishedPricingHandler"/> query: the public pricing page
/// sees only published products, each with its pricing shape, plans and prices.
/// </summary>
public class ListPublishedPricingHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPublishedTieredProductWithItsPlansAndPrices()
    {
        var products = new InMemoryProductRepository();

        var workflows = Product.CreateDraft(
            Slug.Create("workflows"), "Workflows.AI", "Business management", "All-in-one platform.");
        workflows.AddPlan(
            "Solo", "For an owner-operator.", Money.Create(29m, Currency.Usd), BillingPeriod.Monthly, ["1 user"]);
        var team = workflows.AddPlan(
            "Team", "For small crews.", Money.Create(79m, Currency.Usd), BillingPeriod.Monthly, ["Up to 5 users"]);
        workflows.FeaturePlan(team);
        workflows.Publish();
        await products.AddAsync(workflows);

        var result = await new ListPublishedPricingHandler(products).Handle(CancellationToken.None);

        var product = Assert.Single(result);
        Assert.Equal("workflows", product.Slug);
        Assert.Equal("Workflows.AI", product.Name);
        Assert.Equal(nameof(PricingKind.Tiered), product.PricingKind);
        Assert.Equal(2, product.Plans.Count);

        var solo = product.Plans[0];
        Assert.Equal("Solo", solo.Name);
        Assert.Equal(29m, solo.Amount);
        Assert.Equal(nameof(Currency.Usd), solo.Currency);
        Assert.Equal(nameof(BillingPeriod.Monthly), solo.BillingPeriod);
        Assert.False(solo.IsFeatured);

        Assert.True(product.Plans[1].IsFeatured);
    }

    [Fact]
    public async Task Handle_ExcludesDraftProducts()
    {
        var products = new InMemoryProductRepository();
        await products.AddAsync(Product.CreateDraft(
            Slug.Create("draft"), "Draft", "Category", "Summary."));

        var result = await new ListPublishedPricingHandler(products).Handle(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsQuoteBasedProductWithNoPlans()
    {
        var products = new InMemoryProductRepository();
        var airCompliance = Product.CreateDraft(
            Slug.Create("air-compliance"), "Air Compliance", "Environmental", "Quote-based product.");
        airCompliance.MakeQuoteBased();
        airCompliance.Publish();
        await products.AddAsync(airCompliance);

        var result = await new ListPublishedPricingHandler(products).Handle(CancellationToken.None);

        var product = Assert.Single(result);
        Assert.Equal(nameof(PricingKind.Quote), product.PricingKind);
        Assert.Empty(product.Plans);
    }
}
