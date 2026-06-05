using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Products;

/// <summary>Behaviour of the <see cref="SetProductPublishedHandler"/> admin use case.</summary>
public class SetProductPublishedHandlerTests
{
    [Fact]
    public async Task Handle_PublishesADraftProduct()
    {
        var products = new InMemoryProductRepository();
        var product = Product.CreateDraft(Slug.Create("workflows"), "Workflows", "Tools", "Summary.");
        product.AddPlan("Solo", "", Money.Create(29m, Currency.Usd), BillingPeriod.Monthly);
        await products.AddAsync(product);
        var handler = new SetProductPublishedHandler(products);

        await handler.Handle(new SetProductPublishedCommand(product.Id.Value, Published: true), CancellationToken.None);

        var saved = await products.GetByIdAsync(product.Id);
        Assert.Equal(ProductStatus.Published, saved!.Status);
    }

    [Fact]
    public async Task Handle_UnpublishesAPublishedProduct()
    {
        var products = new InMemoryProductRepository();
        var product = Product.CreateDraft(Slug.Create("workflows"), "Workflows", "Tools", "Summary.");
        product.MakeQuoteBased();
        product.Publish();
        await products.AddAsync(product);
        var handler = new SetProductPublishedHandler(products);

        await handler.Handle(new SetProductPublishedCommand(product.Id.Value, Published: false), CancellationToken.None);

        var saved = await products.GetByIdAsync(product.Id);
        Assert.Equal(ProductStatus.Draft, saved!.Status);
    }

    [Fact]
    public async Task Handle_ForUnknownProduct_ThrowsNotFound()
    {
        var products = new InMemoryProductRepository();
        var handler = new SetProductPublishedHandler(products);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new SetProductPublishedCommand(Guid.NewGuid(), Published: true), CancellationToken.None));
    }
}
