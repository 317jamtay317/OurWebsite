using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Products;

/// <summary>
/// Behaviour of the <see cref="GetPublishedProductHandler"/> query: returns the public
/// page model for a published product addressed by its slug, and nothing for products
/// that are missing, still drafts, or addressed by a slug that is not valid.
/// </summary>
public class GetPublishedProductHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTieredProductWithItsPlans_WhenPublished()
    {
        var products = new InMemoryProductRepository();
        var product = Product.CreateDraft(
            Slug.Create("workflows"), "Workflows.AI", "Business management", "All-in-one platform.");
        product.AddPlan(
            "Solo", "For an owner-operator.", Money.Create(29m, Currency.Usd), BillingPeriod.Monthly, ["1 user"]);
        var team = product.AddPlan(
            "Team", "For crews.", Money.Create(79m, Currency.Usd), BillingPeriod.Monthly, ["Up to 5 users", "Automation"]);
        product.FeaturePlan(team);
        product.Publish();
        await products.AddAsync(product);

        var page = await new GetPublishedProductHandler(products).Handle("workflows");

        Assert.NotNull(page);
        Assert.Equal("workflows", page!.Slug);
        Assert.Equal("Workflows.AI", page.Name);
        Assert.Equal("Business management", page.Category);
        Assert.Equal(PricingKind.Tiered, page.PricingKind);
        Assert.Null(page.FixedPriceAmount);
        Assert.Equal(2, page.Plans.Count);

        var teamPlan = page.Plans.Single(plan => plan.Name == "Team");
        Assert.Equal(79m, teamPlan.Amount);
        Assert.Equal(BillingPeriod.Monthly, teamPlan.BillingPeriod);
        Assert.True(teamPlan.IsFeatured);
        Assert.Equal(new[] { "Up to 5 users", "Automation" }, teamPlan.Features);
    }

    [Fact]
    public async Task Handle_ReturnsFixedPriceProduct_WhenPublished()
    {
        var products = new InMemoryProductRepository();
        var product = Product.CreateDraft(Slug.Create("audit"), "Compliance Audit", "Compliance", "A one-time audit.");
        product.MakeFixedPrice(Money.Create(499m, Currency.Usd));
        product.Publish();
        await products.AddAsync(product);

        var page = await new GetPublishedProductHandler(products).Handle("audit");

        Assert.NotNull(page);
        Assert.Equal(PricingKind.Fixed, page!.PricingKind);
        Assert.Equal(499m, page.FixedPriceAmount);
        Assert.Empty(page.Plans);
    }

    [Fact]
    public async Task Handle_ReturnsQuoteBasedProduct_WhenPublished()
    {
        var products = new InMemoryProductRepository();
        var product = Product.CreateDraft(
            Slug.Create("air-compliance"), "Air Compliance", "Environmental compliance", "Quote-based product.");
        product.MakeQuoteBased();
        product.Publish();
        await products.AddAsync(product);

        var page = await new GetPublishedProductHandler(products).Handle("air-compliance");

        Assert.NotNull(page);
        Assert.Equal(PricingKind.Quote, page!.PricingKind);
        Assert.Null(page.FixedPriceAmount);
        Assert.Empty(page.Plans);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenTheProductIsStillADraft()
    {
        var products = new InMemoryProductRepository();
        await products.AddAsync(Product.CreateDraft(
            Slug.Create("draft-product"), "Draft", "Category", "Summary."));

        Assert.Null(await new GetPublishedProductHandler(products).Handle("draft-product"));
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenNoProductHasTheSlug()
    {
        var products = new InMemoryProductRepository();

        Assert.Null(await new GetPublishedProductHandler(products).Handle("nope"));
    }

    [Theory]
    [InlineData("Workflows")]   // uppercase is not a valid slug
    [InlineData("not a slug")]  // spaces are not valid
    [InlineData("")]            // empty is not valid
    public async Task Handle_ReturnsNull_WhenTheSlugIsNotValid(string slug)
    {
        var products = new InMemoryProductRepository();

        Assert.Null(await new GetPublishedProductHandler(products).Handle(slug));
    }
}
