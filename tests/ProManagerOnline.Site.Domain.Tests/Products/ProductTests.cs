using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Domain.Tests.Products;

/// <summary>
/// Behaviour of the <see cref="Product"/> aggregate root: how a product is drafted,
/// priced (tiered plans or quote-based), and published.
/// </summary>
public class ProductTests
{
    private static Product NewDraft() =>
        Product.CreateDraft(
            Slug.Create("workflows"),
            name: "Workflows.AI",
            category: "Business management",
            summary: "All-in-one platform for small teams.");

    private static Money Usd(decimal amount) => Money.Create(amount, Currency.Usd);

    [Fact]
    public void CreateDraft_StartsAsADraftTieredProductWithNoPlans()
    {
        var product = NewDraft();

        Assert.Equal(ProductStatus.Draft, product.Status);
        Assert.Equal(PricingKind.Tiered, product.PricingKind);
        Assert.Empty(product.Plans);
        Assert.Equal("workflows", product.Slug.Value);
        Assert.Equal("Workflows.AI", product.Name);
    }

    [Fact]
    public void CreateDraft_GivenBlankName_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Product.CreateDraft(Slug.Create("workflows"), "  ", "Business management", "Summary."));
    }

    [Fact]
    public void AddPlan_AddsPlanWithGivenDetails()
    {
        var product = NewDraft();

        var planId = product.AddPlan("Solo", "For an owner-operator.", Usd(29m), BillingPeriod.Monthly);

        var plan = Assert.Single(product.Plans);
        Assert.Equal(planId, plan.Id);
        Assert.Equal("Solo", plan.Name);
        Assert.Equal(29m, plan.Price.Amount);
        Assert.Equal(BillingPeriod.Monthly, plan.BillingPeriod);
        Assert.False(plan.IsFeatured);
    }

    [Fact]
    public void ChangePlanPrice_UpdatesThatPlansPrice()
    {
        var product = NewDraft();
        var planId = product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly);

        product.ChangePlanPrice(planId, Usd(39m));

        Assert.Equal(39m, product.Plans.Single(p => p.Id == planId).Price.Amount);
    }

    [Fact]
    public void ChangePlanPrice_ForUnknownPlan_ThrowsDomainException()
    {
        var product = NewDraft();

        Assert.Throws<DomainException>(() => product.ChangePlanPrice(PlanId.New(), Usd(39m)));
    }

    [Fact]
    public void FeaturePlan_MarksItFeaturedAndClearsAnyOther()
    {
        var product = NewDraft();
        var solo = product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly);
        var team = product.AddPlan("Team", "", Usd(79m), BillingPeriod.Monthly);

        product.FeaturePlan(solo);
        product.FeaturePlan(team);

        Assert.False(product.Plans.Single(p => p.Id == solo).IsFeatured);
        Assert.True(product.Plans.Single(p => p.Id == team).IsFeatured);
    }

    [Fact]
    public void Publish_TieredProductWithNoPlans_ThrowsDomainException()
    {
        var product = NewDraft();

        Assert.Throws<DomainException>(product.Publish);
    }

    [Fact]
    public void Publish_TieredProductWithAtLeastOnePlan_BecomesPublished()
    {
        var product = NewDraft();
        product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly);

        product.Publish();

        Assert.Equal(ProductStatus.Published, product.Status);
    }

    [Fact]
    public void Unpublish_ReturnsAPublishedProductToDraft()
    {
        var product = NewDraft();
        product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly);
        product.Publish();

        product.Unpublish();

        Assert.Equal(ProductStatus.Draft, product.Status);
    }

    [Fact]
    public void Unpublish_OnADraftProduct_LeavesItDraft()
    {
        var product = NewDraft();

        product.Unpublish();

        Assert.Equal(ProductStatus.Draft, product.Status);
    }

    [Fact]
    public void MakeQuoteBased_SwitchesKindAndAllowsPublishingWithoutPlans()
    {
        var product = NewDraft();

        product.MakeQuoteBased();
        product.Publish();

        Assert.Equal(PricingKind.Quote, product.PricingKind);
        Assert.Equal(ProductStatus.Published, product.Status);
        Assert.Empty(product.Plans);
    }

    [Fact]
    public void MakeQuoteBased_DiscardsExistingPlans()
    {
        var product = NewDraft();
        product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly);

        product.MakeQuoteBased();

        Assert.Empty(product.Plans);
    }

    [Fact]
    public void AddPlan_WhenQuoteBased_ThrowsDomainException()
    {
        var product = NewDraft();
        product.MakeQuoteBased();

        Assert.Throws<DomainException>(() => product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly));
    }

    // ---- Plan features ----

    [Fact]
    public void AddPlan_WithFeatures_StoresThemInOrder()
    {
        var product = NewDraft();

        var planId = product.AddPlan(
            "Pro", "For growing teams.", Usd(79m), BillingPeriod.Monthly,
            ["Unlimited projects", "Priority support", "Advanced reports"]);

        var plan = product.Plans.Single(p => p.Id == planId);
        Assert.Equal(new[] { "Unlimited projects", "Priority support", "Advanced reports" }, plan.Features);
    }

    [Fact]
    public void AddPlan_TrimsFeaturesAndDropsBlankOnes()
    {
        var product = NewDraft();

        var planId = product.AddPlan(
            "Pro", "", Usd(79m), BillingPeriod.Monthly,
            ["  Unlimited projects  ", "   ", "", "Priority support"]);

        var plan = product.Plans.Single(p => p.Id == planId);
        Assert.Equal(new[] { "Unlimited projects", "Priority support" }, plan.Features);
    }

    [Fact]
    public void UpdatePlan_ChangesDetailsAndFeatures()
    {
        var product = NewDraft();
        var planId = product.AddPlan("Solo", "old", Usd(29m), BillingPeriod.Monthly, ["A"]);

        product.UpdatePlan(planId, "Solo Plus", "new", Usd(39m), BillingPeriod.Annual, ["B", "C"]);

        var plan = product.Plans.Single(p => p.Id == planId);
        Assert.Equal("Solo Plus", plan.Name);
        Assert.Equal("new", plan.Description);
        Assert.Equal(39m, plan.Price.Amount);
        Assert.Equal(BillingPeriod.Annual, plan.BillingPeriod);
        Assert.Equal(new[] { "B", "C" }, plan.Features);
    }

    [Fact]
    public void UpdatePlan_ForUnknownPlan_ThrowsDomainException()
    {
        var product = NewDraft();

        Assert.Throws<DomainException>(() =>
            product.UpdatePlan(PlanId.New(), "X", "", Usd(1m), BillingPeriod.Monthly, []));
    }

    [Fact]
    public void RemovePlan_RemovesThatPlan()
    {
        var product = NewDraft();
        var solo = product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly);
        var team = product.AddPlan("Team", "", Usd(79m), BillingPeriod.Monthly);

        product.RemovePlan(solo);

        Assert.Equal(team, product.Plans.Single().Id);
    }

    [Fact]
    public void RemovePlan_ForUnknownPlan_ThrowsDomainException()
    {
        var product = NewDraft();

        Assert.Throws<DomainException>(() => product.RemovePlan(PlanId.New()));
    }

    // ---- Fixed price ----

    [Fact]
    public void MakeFixedPrice_SetsKindAndPriceAndClearsPlans()
    {
        var product = NewDraft();
        product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly);

        product.MakeFixedPrice(Usd(499m));

        Assert.Equal(PricingKind.Fixed, product.PricingKind);
        Assert.NotNull(product.FixedPrice);
        Assert.Equal(499m, product.FixedPrice!.Amount);
        Assert.Empty(product.Plans);
    }

    [Fact]
    public void MakeFixedPrice_ThenPublish_BecomesPublished()
    {
        var product = NewDraft();
        product.MakeFixedPrice(Usd(499m));

        product.Publish();

        Assert.Equal(ProductStatus.Published, product.Status);
    }

    [Fact]
    public void ChangeFixedPrice_UpdatesThePrice()
    {
        var product = NewDraft();
        product.MakeFixedPrice(Usd(499m));

        product.ChangeFixedPrice(Usd(599m));

        Assert.Equal(599m, product.FixedPrice!.Amount);
    }

    [Fact]
    public void ChangeFixedPrice_WhenNotFixed_ThrowsDomainException()
    {
        var product = NewDraft();

        Assert.Throws<DomainException>(() => product.ChangeFixedPrice(Usd(599m)));
    }

    [Fact]
    public void MakeQuoteBased_ClearsFixedPrice()
    {
        var product = NewDraft();
        product.MakeFixedPrice(Usd(499m));

        product.MakeQuoteBased();

        Assert.Equal(PricingKind.Quote, product.PricingKind);
        Assert.Null(product.FixedPrice);
    }

    [Fact]
    public void MakeTiered_FromFixed_SwitchesKindAndClearsFixedPrice()
    {
        var product = NewDraft();
        product.MakeFixedPrice(Usd(499m));

        product.MakeTiered();

        Assert.Equal(PricingKind.Tiered, product.PricingKind);
        Assert.Null(product.FixedPrice);
    }

    // ---- Editing details, slug and publication ----

    [Fact]
    public void UpdateDetails_ChangesNameCategoryAndSummary()
    {
        var product = NewDraft();

        product.UpdateDetails("New name", "New category", "New summary.");

        Assert.Equal("New name", product.Name);
        Assert.Equal("New category", product.Category);
        Assert.Equal("New summary.", product.Summary);
    }

    [Fact]
    public void UpdateDetails_GivenBlankName_ThrowsDomainException()
    {
        var product = NewDraft();

        Assert.Throws<DomainException>(() => product.UpdateDetails(" ", "Category", "Summary."));
    }

    [Fact]
    public void ChangeSlug_WhileDraft_ChangesTheSlug()
    {
        var product = NewDraft();

        product.ChangeSlug(Slug.Create("new-slug"));

        Assert.Equal("new-slug", product.Slug.Value);
    }

    [Fact]
    public void ChangeSlug_WhilePublished_ThrowsDomainException()
    {
        var product = NewDraft();
        product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly);
        product.Publish();

        Assert.Throws<DomainException>(() => product.ChangeSlug(Slug.Create("new-slug")));
    }

    [Fact]
    public void Unpublish_ReturnsProductToDraft()
    {
        var product = NewDraft();
        product.AddPlan("Solo", "", Usd(29m), BillingPeriod.Monthly);
        product.Publish();

        product.Unpublish();

        Assert.Equal(ProductStatus.Draft, product.Status);
    }
}
