using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Products;

/// <summary>Behaviour of the <see cref="ListAllProductsHandler"/> admin use case.</summary>
public class ListAllProductsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllProductsIncludingDrafts()
    {
        var products = new InMemoryProductRepository();
        await products.AddAsync(Product.CreateDraft(Slug.Create("draft-app"), "Draft App", "Tools", "A draft."));
        var live = Product.CreateDraft(Slug.Create("live-app"), "Live App", "Tools", "Published.");
        live.MakeQuoteBased();
        live.Publish();
        await products.AddAsync(live);
        var handler = new ListAllProductsHandler(products);

        var all = await handler.Handle(CancellationToken.None);

        Assert.Equal(2, all.Count);
        Assert.Contains(all, p => p.Slug == "draft-app" && p.Status == nameof(ProductStatus.Draft));
        Assert.Contains(all, p => p.Slug == "live-app" && p.Status == nameof(ProductStatus.Published));
    }
}
