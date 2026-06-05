using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Documentation;

/// <summary>
/// Behaviour of the <see cref="GetDocArticleHandler"/> query: it returns a single published
/// article for a published product, and nothing otherwise.
/// </summary>
public class GetDocArticleHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsThePublishedArticle()
    {
        var product = DocTestData.PublishedProduct();
        var products = new InMemoryProductRepository();
        await products.AddAsync(product);
        var docs = new InMemoryDocArticleRepository();
        docs.Add(DocTestData.PublishedArticle(
            product.Id, "getting-started", "Getting started", "Basics", 0, "# Hello\n\nWelcome."));

        var page = await new GetDocArticleHandler(products, docs)
            .Handle(product.Id.Value, "getting-started", CancellationToken.None);

        Assert.NotNull(page);
        Assert.Equal("getting-started", page!.Slug);
        Assert.Equal("Getting started", page.Title);
        Assert.Equal("Basics", page.Section);
        Assert.Equal("# Hello\n\nWelcome.", page.Body);
    }

    [Fact]
    public async Task Handle_WhenArticleIsDraft_ReturnsNull()
    {
        var product = DocTestData.PublishedProduct();
        var products = new InMemoryProductRepository();
        await products.AddAsync(product);
        var docs = new InMemoryDocArticleRepository();
        docs.Add(DocTestData.DraftArticle(product.Id, "getting-started", "Getting started", "Basics", 0));

        var page = await new GetDocArticleHandler(products, docs)
            .Handle(product.Id.Value, "getting-started", CancellationToken.None);

        Assert.Null(page);
    }

    [Fact]
    public async Task Handle_WhenProductIsDraft_ReturnsNull()
    {
        var draft = Product.CreateDraft(Slug.Create("draft-product"), "Draft", "Category", "Summary.");
        var products = new InMemoryProductRepository();
        await products.AddAsync(draft);
        var docs = new InMemoryDocArticleRepository();
        docs.Add(DocTestData.PublishedArticle(draft.Id, "getting-started", "Getting started", "Basics", 0));

        var page = await new GetDocArticleHandler(products, docs)
            .Handle(draft.Id.Value, "getting-started", CancellationToken.None);

        Assert.Null(page);
    }

    [Fact]
    public async Task Handle_WhenSlugIsUnknown_ReturnsNull()
    {
        var product = DocTestData.PublishedProduct();
        var products = new InMemoryProductRepository();
        await products.AddAsync(product);

        var page = await new GetDocArticleHandler(products, new InMemoryDocArticleRepository())
            .Handle(product.Id.Value, "no-such-page", CancellationToken.None);

        Assert.Null(page);
    }

    [Fact]
    public async Task Handle_WhenSlugIsMalformed_ReturnsNull()
    {
        var product = DocTestData.PublishedProduct();
        var products = new InMemoryProductRepository();
        await products.AddAsync(product);

        // A route value such as "/docs/{guid}/Not A Slug" is not a valid slug; the handler
        // treats it as "no such article" rather than failing.
        var page = await new GetDocArticleHandler(products, new InMemoryDocArticleRepository())
            .Handle(product.Id.Value, "Not A Slug", CancellationToken.None);

        Assert.Null(page);
    }
}
