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

    public void Dispose() => _connection.Dispose();
}
