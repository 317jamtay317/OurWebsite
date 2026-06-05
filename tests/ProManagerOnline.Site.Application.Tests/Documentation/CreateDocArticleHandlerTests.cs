using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Documentation;

/// <summary>
/// Behaviour of the <see cref="CreateDocArticleHandler"/> use case: it creates a draft article
/// and enforces that a slug is unique within its product.
/// </summary>
public class CreateDocArticleHandlerTests
{
    [Fact]
    public async Task Handle_CreatesADraftArticleAndPersistsIt()
    {
        var productId = ProductId.New();
        var docs = new InMemoryDocArticleRepository();
        var handler = new CreateDocArticleHandler(docs);
        var command = new CreateDocArticleCommand(
            productId.Value, "getting-started", "Getting started", "Basics", "# Hello\n\nWelcome.", 0);

        var id = await handler.Handle(command, CancellationToken.None);

        var saved = await docs.GetByIdAsync(id);
        Assert.NotNull(saved);
        Assert.Equal(productId, saved!.ProductId);
        Assert.Equal("getting-started", saved.Slug.Value);
        Assert.Equal("Getting started", saved.Title);
        Assert.Equal("Basics", saved.Section);
        Assert.Equal("# Hello\n\nWelcome.", saved.Body);
        Assert.Equal(0, saved.Position);
        Assert.Equal(DocStatus.Draft, saved.Status);
        Assert.Equal(1, docs.SaveChangesCount);
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyUsedWithinProduct_ThrowsConflict()
    {
        var productId = ProductId.New();
        var docs = new InMemoryDocArticleRepository();
        docs.Add(DocTestData.DraftArticle(productId, "getting-started", "Existing", "Basics", 0));
        var handler = new CreateDocArticleHandler(docs);
        var command = new CreateDocArticleCommand(
            productId.Value, "getting-started", "Another", "Basics", "Body.", 1);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AllowsTheSameSlugForDifferentProducts()
    {
        var productA = ProductId.New();
        var productB = ProductId.New();
        var docs = new InMemoryDocArticleRepository();
        docs.Add(DocTestData.DraftArticle(productA, "getting-started", "Product A page", "Basics", 0));
        var handler = new CreateDocArticleHandler(docs);
        var command = new CreateDocArticleCommand(
            productB.Value, "getting-started", "Product B page", "Basics", "Body.", 0);

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(await docs.GetByIdAsync(id));
    }
}
