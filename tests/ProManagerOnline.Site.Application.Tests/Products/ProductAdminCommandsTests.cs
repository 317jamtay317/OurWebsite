using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Products;

/// <summary>
/// Behaviour of the admin command use cases: editing details, switching pricing, deleting,
/// and publishing/unpublishing a product.
/// </summary>
public class ProductAdminCommandsTests
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
    public async Task UpdateDetails_ChangesNameAndSummary()
    {
        var (repo, product) = await WithDraft();
        var command = new UpdateProductDetailsCommand(product.Id.Value, "workflows", "New name", "New cat", "New summary.");

        await new UpdateProductDetailsHandler(repo).Handle(command, CancellationToken.None);

        var saved = await repo.GetByIdAsync(product.Id);
        Assert.Equal("New name", saved!.Name);
        Assert.Equal("New summary.", saved.Summary);
    }

    [Fact]
    public async Task UpdateDetails_KeepingOwnSlug_DoesNotConflict()
    {
        var (repo, product) = await WithDraft();
        var command = new UpdateProductDetailsCommand(product.Id.Value, "workflows", "Renamed", "Business", "Summary.");

        await new UpdateProductDetailsHandler(repo).Handle(command, CancellationToken.None);

        Assert.Equal("Renamed", (await repo.GetByIdAsync(product.Id))!.Name);
    }

    [Fact]
    public async Task UpdateDetails_WhenSlugTakenByAnother_ThrowsConflict()
    {
        var (repo, product) = await WithDraft();
        await repo.AddAsync(Product.CreateDraft(Slug.Create("taken"), "Other", "Cat", "Summary."));
        var command = new UpdateProductDetailsCommand(product.Id.Value, "taken", "Workflows.AI", "Business", "Summary.");

        await Assert.ThrowsAsync<ConflictException>(() =>
            new UpdateProductDetailsHandler(repo).Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateDetails_WhenMissing_ThrowsNotFound()
    {
        var repo = new InMemoryProductRepository();
        var command = new UpdateProductDetailsCommand(Guid.NewGuid(), "slug", "Name", "Cat", "Summary.");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new UpdateProductDetailsHandler(repo).Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task SetPricing_ToFixed_SetsFixedPrice()
    {
        var (repo, product) = await WithDraft();
        var command = new SetProductPricingCommand(product.Id.Value, PricingKind.Fixed, 499m);

        await new SetProductPricingHandler(repo).Handle(command, CancellationToken.None);

        var saved = await repo.GetByIdAsync(product.Id);
        Assert.Equal(PricingKind.Fixed, saved!.PricingKind);
        Assert.Equal(499m, saved.FixedPrice!.Amount);
    }

    [Fact]
    public async Task SetPricing_ToTiered_ClearsFixedPrice()
    {
        var (repo, product) = await WithDraft();
        product.MakeFixedPrice(Usd(499m));
        await repo.UpdateAsync(product);

        await new SetProductPricingHandler(repo).Handle(
            new SetProductPricingCommand(product.Id.Value, PricingKind.Tiered, null), CancellationToken.None);

        var saved = await repo.GetByIdAsync(product.Id);
        Assert.Equal(PricingKind.Tiered, saved!.PricingKind);
        Assert.Null(saved.FixedPrice);
    }

    [Fact]
    public async Task Delete_RemovesProduct()
    {
        var (repo, product) = await WithDraft();

        await new DeleteProductHandler(repo).Handle(product.Id.Value, CancellationToken.None);

        Assert.Null(await repo.GetByIdAsync(product.Id));
    }

    [Fact]
    public async Task Publish_ThenUnpublish_TogglesStatus()
    {
        var (repo, product) = await WithDraft();
        product.AddPlan("Team", "", Usd(79m), BillingPeriod.Monthly);
        await repo.UpdateAsync(product);

        await new PublishProductHandler(repo).Handle(product.Id.Value, CancellationToken.None);
        Assert.Equal(ProductStatus.Published, (await repo.GetByIdAsync(product.Id))!.Status);

        await new UnpublishProductHandler(repo).Handle(product.Id.Value, CancellationToken.None);
        Assert.Equal(ProductStatus.Draft, (await repo.GetByIdAsync(product.Id))!.Status);
    }

    [Fact]
    public async Task Publish_WhenMissing_ThrowsNotFound()
    {
        var repo = new InMemoryProductRepository();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new PublishProductHandler(repo).Handle(Guid.NewGuid(), CancellationToken.None));
    }
}
