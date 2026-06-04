using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Products;

/// <summary>
/// Behaviour of the admin read queries: <see cref="ListProductsHandler"/> (all products,
/// including drafts) and <see cref="GetProductHandler"/> (full detail for editing).
/// </summary>
public class ProductAdminQueriesTests
{
    private static Money Usd(decimal amount) => Money.Create(amount, Currency.Usd);

    [Fact]
    public async Task ListProducts_ReturnsAllProductsIncludingDrafts()
    {
        var products = new InMemoryProductRepository();

        var published = Product.CreateDraft(Slug.Create("workflows"), "Workflows.AI", "Business", "Summary.");
        published.AddPlan("Team", "", Usd(79m), BillingPeriod.Monthly);
        published.Publish();
        await products.AddAsync(published);
        await products.AddAsync(Product.CreateDraft(Slug.Create("draft"), "Draft", "Cat", "Summary."));

        var result = await new ListProductsHandler(products).Handle(CancellationToken.None);

        Assert.Equal(2, result.Count);
        var workflows = result.Single(p => p.Slug == "workflows");
        Assert.Equal(ProductStatus.Published, workflows.Status);
        Assert.Equal(PricingKind.Tiered, workflows.PricingKind);
        Assert.Equal(1, workflows.PlanCount);
    }

    [Fact]
    public async Task GetProduct_ReturnsDetailWithPlansAndFeatures()
    {
        var products = new InMemoryProductRepository();
        var product = Product.CreateDraft(Slug.Create("workflows"), "Workflows.AI", "Business", "Summary.");
        var planId = product.AddPlan("Pro", "For teams.", Usd(79m), BillingPeriod.Monthly, ["Unlimited projects", "Priority support"]);
        product.FeaturePlan(planId);
        await products.AddAsync(product);

        var detail = await new GetProductHandler(products).Handle(product.Id.Value, CancellationToken.None);

        Assert.NotNull(detail);
        Assert.Equal("workflows", detail!.Slug);
        var plan = Assert.Single(detail.Plans);
        Assert.Equal("Pro", plan.Name);
        Assert.True(plan.IsFeatured);
        Assert.Equal(new[] { "Unlimited projects", "Priority support" }, plan.Features);
    }

    [Fact]
    public async Task GetProduct_ForFixedPriceProduct_IncludesFixedPriceAmount()
    {
        var products = new InMemoryProductRepository();
        var product = Product.CreateDraft(Slug.Create("audit"), "Audit", "Compliance", "Summary.");
        product.MakeFixedPrice(Usd(499m));
        await products.AddAsync(product);

        var detail = await new GetProductHandler(products).Handle(product.Id.Value, CancellationToken.None);

        Assert.NotNull(detail);
        Assert.Equal(PricingKind.Fixed, detail!.PricingKind);
        Assert.Equal(499m, detail.FixedPriceAmount);
        Assert.Empty(detail.Plans);
    }

    [Fact]
    public async Task GetProduct_WhenMissing_ReturnsNull()
    {
        var products = new InMemoryProductRepository();

        var detail = await new GetProductHandler(products).Handle(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(detail);
    }
}
