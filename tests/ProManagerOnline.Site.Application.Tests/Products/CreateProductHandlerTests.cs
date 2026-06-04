using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Application.Products;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Products;

/// <summary>
/// Behaviour of the <see cref="CreateProductHandler"/> use case.
/// </summary>
public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_CreatesADraftProductAndPersistsIt()
    {
        var products = new InMemoryProductRepository();
        var handler = new CreateProductHandler(products);
        var command = new CreateProductCommand(
            "workflows", "Workflows.AI", "Business management", "All-in-one platform for small teams.");

        var productId = await handler.Handle(command, CancellationToken.None);

        var saved = await products.GetByIdAsync(productId);
        Assert.NotNull(saved);
        Assert.Equal("workflows", saved!.Slug.Value);
        Assert.Equal("Workflows.AI", saved.Name);
        Assert.Equal(ProductStatus.Draft, saved.Status);
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyInUse_ThrowsConflict()
    {
        var products = new InMemoryProductRepository();
        await products.AddAsync(Product.CreateDraft(Slug.Create("workflows"), "Existing", "Category", "Summary."));
        var handler = new CreateProductHandler(products);
        var command = new CreateProductCommand(
            "workflows", "Workflows.AI", "Business management", "All-in-one platform.");

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
    }
}
