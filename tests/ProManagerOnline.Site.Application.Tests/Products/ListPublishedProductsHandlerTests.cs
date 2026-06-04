using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Products;

/// <summary>
/// Behaviour of the <see cref="ListPublishedProductsHandler"/> query: only published
/// products are returned, for the public site.
/// </summary>
public class ListPublishedProductsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyPublishedProducts()
    {
        var products = new InMemoryProductRepository();

        var published = Product.CreateDraft(
            Slug.Create("workflows"), "Workflows.AI", "Business management", "All-in-one platform.");
        published.AddPlan("Team", "", Money.Create(79m, Currency.Usd), BillingPeriod.Monthly);
        published.Publish();
        await products.AddAsync(published);

        await products.AddAsync(Product.CreateDraft(
            Slug.Create("draft-product"), "Draft", "Category", "Summary."));

        var result = await new ListPublishedProductsHandler(products).Handle(CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("workflows", dto.Slug);
        Assert.Equal("Workflows.AI", dto.Name);
        Assert.Equal("Business management", dto.Category);
    }
}
