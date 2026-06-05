using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Products;

/// <summary>
/// Behaviour of the plan (tier) use cases on a subscription product: adding, updating,
/// removing and featuring a plan.
/// </summary>
public class PlanHandlersTests
{
    private static Money Usd(decimal amount) => Money.Create(amount, Currency.Usd);

    private static async Task<(InMemoryProductRepository Repo, Product Product)> WithDraft()
    {
        var repo = new InMemoryProductRepository();
        var product = Product.CreateDraft(Slug.Create("workflows"), "Workflows.AI", "Business", "Summary.");
        await repo.AddAsync(product);
        return (repo, product);
    }

    [Fact]
    public async Task AddPlan_AddsPlanWithFeatures()
    {
        var (repo, product) = await WithDraft();
        var command = new AddPlanCommand(
            product.Id.Value, "Pro", "For teams.", 79m, BillingPeriod.Monthly, ["Unlimited projects", "Priority support"]);

        await new AddPlanHandler(repo).Handle(command, CancellationToken.None);

        var plan = Assert.Single((await repo.GetByIdAsync(product.Id))!.Plans);
        Assert.Equal("Pro", plan.Name);
        Assert.Equal(79m, plan.Price.Amount);
        Assert.Equal(new[] { "Unlimited projects", "Priority support" }, plan.Features);
    }

    [Fact]
    public async Task UpdatePlan_ChangesPlanDetailsAndFeatures()
    {
        var (repo, product) = await WithDraft();
        var planId = product.AddPlan("Solo", "old", Usd(29m), BillingPeriod.Monthly, ["A"]);
        await repo.UpdateAsync(product);

        var command = new UpdatePlanCommand(
            product.Id.Value, planId.Value, "Solo Plus", "new", 39m, BillingPeriod.Annual, ["B", "C"]);
        await new UpdatePlanHandler(repo).Handle(command, CancellationToken.None);

        var plan = Assert.Single((await repo.GetByIdAsync(product.Id))!.Plans);
        Assert.Equal("Solo Plus", plan.Name);
        Assert.Equal(39m, plan.Price.Amount);
        Assert.Equal(new[] { "B", "C" }, plan.Features);
    }

    [Fact]
    public async Task RemovePlan_RemovesThePlan()
    {
        var (repo, product) = await WithDraft();
        var solo = product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly);
        product.AddPlan("Team", "", Usd(79m), BillingPeriod.Monthly);
        await repo.UpdateAsync(product);

        await new RemovePlanHandler(repo).Handle(
            new RemovePlanCommand(product.Id.Value, solo.Value), CancellationToken.None);

        var plan = Assert.Single((await repo.GetByIdAsync(product.Id))!.Plans);
        Assert.Equal("Team", plan.Name);
    }

    [Fact]
    public async Task FeaturePlan_MarksItFeatured()
    {
        var (repo, product) = await WithDraft();
        var solo = product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly);
        await repo.UpdateAsync(product);

        await new FeaturePlanHandler(repo).Handle(
            new FeaturePlanCommand(product.Id.Value, solo.Value), CancellationToken.None);

        Assert.True((await repo.GetByIdAsync(product.Id))!.Plans.Single().IsFeatured);
    }
}
