using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using ProManagerOnline.Site.Infrastructure.Persistence;
using Xunit;

namespace ProManagerOnline.Site.Infrastructure.Tests;

/// <summary>
/// Integration tests for <see cref="ProductRepository"/> and the EF Core mapping of the
/// <see cref="Product"/> aggregate, exercised against a real (in-memory SQLite) database.
/// </summary>
public sealed class ProductRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SiteDbContext> _options;

    public ProductRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<SiteDbContext>().UseSqlite(_connection).Options;

        using var context = new SiteDbContext(_options);
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task AddAndGetById_RoundTripsTheProductWithItsPlan()
    {
        var product = Product.CreateDraft(
            Slug.Create("workflows"), "Workflows.AI", "Business management", "All-in-one platform.");
        var planId = product.AddPlan("Team", "For small crews.", Money.Create(79m, Currency.Usd), BillingPeriod.Monthly);
        product.FeaturePlan(planId);
        product.Publish();

        await using (var context = new SiteDbContext(_options))
        {
            await new ProductRepository(context).AddAsync(product);
        }

        await using (var context = new SiteDbContext(_options))
        {
            var loaded = await new ProductRepository(context).GetByIdAsync(product.Id);

            Assert.NotNull(loaded);
            Assert.Equal("workflows", loaded!.Slug.Value);
            Assert.Equal(ProductStatus.Published, loaded.Status);

            var plan = Assert.Single(loaded.Plans);
            Assert.Equal("Team", plan.Name);
            Assert.Equal(79m, plan.Price.Amount);
            Assert.Equal(Currency.Usd, plan.Price.Currency);
            Assert.True(plan.IsFeatured);
        }
    }

    [Fact]
    public async Task SlugExists_ReflectsSavedProducts()
    {
        await using var context = new SiteDbContext(_options);
        await new ProductRepository(context).AddAsync(
            Product.CreateDraft(Slug.Create("workflows"), "Workflows.AI", "Business management", "Summary."));

        var repository = new ProductRepository(context);
        Assert.True(await repository.SlugExistsAsync(Slug.Create("workflows")));
        Assert.False(await repository.SlugExistsAsync(Slug.Create("air-compliance")));
    }

    [Fact]
    public async Task AddAndGetById_RoundTripsPlanFeaturesInOrder()
    {
        var product = Product.CreateDraft(Slug.Create("workflows"), "Workflows.AI", "Business", "Summary.");
        product.AddPlan(
            "Pro", "For teams.", Money.Create(79m, Currency.Usd), BillingPeriod.Monthly,
            ["Unlimited projects", "Priority support", "Advanced reports"]);

        await using (var context = new SiteDbContext(_options))
        {
            await new ProductRepository(context).AddAsync(product);
        }

        await using (var context = new SiteDbContext(_options))
        {
            var loaded = await new ProductRepository(context).GetByIdAsync(product.Id);
            var plan = Assert.Single(loaded!.Plans);
            Assert.Equal(new[] { "Unlimited projects", "Priority support", "Advanced reports" }, plan.Features);
        }
    }

    [Fact]
    public async Task AddAndGetById_RoundTripsFixedPrice()
    {
        var product = Product.CreateDraft(Slug.Create("audit"), "Compliance Audit", "Compliance", "Summary.");
        product.MakeFixedPrice(Money.Create(499m, Currency.Usd));

        await using (var context = new SiteDbContext(_options))
        {
            await new ProductRepository(context).AddAsync(product);
        }

        await using (var context = new SiteDbContext(_options))
        {
            var loaded = await new ProductRepository(context).GetByIdAsync(product.Id);
            Assert.Equal(PricingKind.Fixed, loaded!.PricingKind);
            Assert.NotNull(loaded.FixedPrice);
            Assert.Equal(499m, loaded.FixedPrice!.Amount);
        }
    }

    [Fact]
    public async Task AddAndGetById_TieredProduct_HasNoFixedPrice()
    {
        var product = Product.CreateDraft(Slug.Create("workflows"), "Workflows.AI", "Business", "Summary.");
        product.AddPlan("Team", "", Money.Create(79m, Currency.Usd), BillingPeriod.Monthly);

        await using (var context = new SiteDbContext(_options))
        {
            await new ProductRepository(context).AddAsync(product);
        }

        await using (var context = new SiteDbContext(_options))
        {
            var loaded = await new ProductRepository(context).GetByIdAsync(product.Id);
            Assert.Null(loaded!.FixedPrice);
        }
    }

    [Fact]
    public async Task Update_AppliesPlanEditsAndRemovals()
    {
        var product = Product.CreateDraft(Slug.Create("workflows"), "Workflows.AI", "Business", "Summary.");
        var solo = product.AddPlan("Solo", "old", Money.Create(29m, Currency.Usd), BillingPeriod.Monthly, ["A"]);
        var team = product.AddPlan("Team", "", Money.Create(79m, Currency.Usd), BillingPeriod.Monthly);

        await using (var context = new SiteDbContext(_options))
        {
            await new ProductRepository(context).AddAsync(product);
        }

        await using (var context = new SiteDbContext(_options))
        {
            var repository = new ProductRepository(context);
            var loaded = await repository.GetByIdAsync(product.Id);
            loaded!.UpdatePlan(solo, "Solo Plus", "new", Money.Create(39m, Currency.Usd), BillingPeriod.Annual, ["B", "C"]);
            loaded.RemovePlan(team);
            await repository.UpdateAsync(loaded);
        }

        await using (var context = new SiteDbContext(_options))
        {
            var loaded = await new ProductRepository(context).GetByIdAsync(product.Id);
            var plan = Assert.Single(loaded!.Plans);
            Assert.Equal("Solo Plus", plan.Name);
            Assert.Equal(39m, plan.Price.Amount);
            Assert.Equal(BillingPeriod.Annual, plan.BillingPeriod);
            Assert.Equal(new[] { "B", "C" }, plan.Features);
        }
    }

    [Fact]
    public async Task Update_PersistsAStatusChange_AndLeavesPlansIntact()
    {
        var product = Product.CreateDraft(Slug.Create("workflows"), "Workflows.AI", "Tools", "Summary.");
        product.AddPlan("Team", "For crews.", Money.Create(79m, Currency.Usd), BillingPeriod.Monthly);

        await using (var context = new SiteDbContext(_options))
        {
            await new ProductRepository(context).AddAsync(product);
        }

        await using (var context = new SiteDbContext(_options))
        {
            var repository = new ProductRepository(context);
            var loaded = await repository.GetByIdAsync(product.Id);
            loaded!.Publish();
            await repository.UpdateAsync(loaded);
        }

        await using (var context = new SiteDbContext(_options))
        {
            var reloaded = await new ProductRepository(context).GetByIdAsync(product.Id);
            Assert.Equal(ProductStatus.Published, reloaded!.Status);
            Assert.Single(reloaded.Plans);
            Assert.Equal("Team", reloaded.Plans.Single().Name);
        }
    }

    [Fact]
    public async Task Remove_DeletesTheProduct()
    {
        var product = Product.CreateDraft(Slug.Create("workflows"), "Workflows.AI", "Business", "Summary.");
        await using (var context = new SiteDbContext(_options))
        {
            await new ProductRepository(context).AddAsync(product);
        }

        await using (var context = new SiteDbContext(_options))
        {
            var repository = new ProductRepository(context);
            await repository.RemoveAsync((await repository.GetByIdAsync(product.Id))!);
        }

        await using (var context = new SiteDbContext(_options))
        {
            Assert.Null(await new ProductRepository(context).GetByIdAsync(product.Id));
        }
    }

    [Fact]
    public async Task ListAll_IncludesDraftsAndPublished()
    {
        await using var context = new SiteDbContext(_options);
        var repository = new ProductRepository(context);
        await repository.AddAsync(Product.CreateDraft(Slug.Create("draft-app"), "Draft", "Tools", "A draft."));
        var live = Product.CreateDraft(Slug.Create("live-app"), "Live", "Tools", "Published.");
        live.MakeQuoteBased();
        live.Publish();
        await repository.AddAsync(live);

        var all = await repository.ListAllAsync();

        Assert.Equal(2, all.Count);
        Assert.Contains(all, p => p.Slug.Value == "draft-app" && p.Status == ProductStatus.Draft);
        Assert.Contains(all, p => p.Slug.Value == "live-app" && p.Status == ProductStatus.Published);
    }

    [Fact]
    public async Task SlugExists_ExcludingProduct_IgnoresThatProduct()
    {
        var product = Product.CreateDraft(Slug.Create("workflows"), "Workflows.AI", "Business", "Summary.");
        await using var context = new SiteDbContext(_options);
        var repository = new ProductRepository(context);
        await repository.AddAsync(product);

        Assert.True(await repository.SlugExistsAsync(Slug.Create("workflows")));
        Assert.False(await repository.SlugExistsAsync(Slug.Create("workflows"), product.Id));
    }

    [Fact]
    public async Task GetBySlug_ReturnsTheProductWithItsPlans_WhenSlugExists()
    {
        var product = Product.CreateDraft(
            Slug.Create("workflows"), "Workflows.AI", "Business management", "All-in-one platform.");
        product.AddPlan(
            "Team", "For small crews.", Money.Create(79m, Currency.Usd), BillingPeriod.Monthly, ["Up to 5 users"]);
        product.Publish();

        await using (var context = new SiteDbContext(_options))
        {
            await new ProductRepository(context).AddAsync(product);
        }

        await using (var context = new SiteDbContext(_options))
        {
            var loaded = await new ProductRepository(context).GetBySlugAsync(Slug.Create("workflows"));

            Assert.NotNull(loaded);
            Assert.Equal(product.Id, loaded!.Id);
            Assert.Equal("Workflows.AI", loaded.Name);
            Assert.Equal("Team", Assert.Single(loaded.Plans).Name);
        }
    }

    [Fact]
    public async Task GetBySlug_ReturnsNull_WhenNoProductHasTheSlug()
    {
        await using var context = new SiteDbContext(_options);
        await new ProductRepository(context).AddAsync(
            Product.CreateDraft(Slug.Create("workflows"), "Workflows.AI", "Business", "Summary."));

        Assert.Null(await new ProductRepository(context).GetBySlugAsync(Slug.Create("air-compliance")));
    }

    public void Dispose() => _connection.Dispose();
}
