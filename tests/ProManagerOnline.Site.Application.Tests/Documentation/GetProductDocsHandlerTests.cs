using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Documentation;

/// <summary>
/// Behaviour of the <see cref="GetProductDocsHandler"/> query: it builds the sidebar
/// navigation for a product's published documentation, grouped into ordered sections.
/// </summary>
public class GetProductDocsHandlerTests
{
    [Fact]
    public async Task Handle_GroupsPublishedArticlesIntoSectionsOrderedByEarliestPosition()
    {
        var product = DocTestData.PublishedProduct();
        var products = new InMemoryProductRepository();
        await products.AddAsync(product);

        var docs = new InMemoryDocArticleRepository();
        // "Getting started" has the earliest position (0), so it must precede "Advanced"
        // even though "Advanced" sorts first alphabetically.
        docs.Add(DocTestData.PublishedArticle(product.Id, "automations", "Automations", "Advanced", 2));
        docs.Add(DocTestData.PublishedArticle(product.Id, "install", "Install", "Getting started", 0));
        docs.Add(DocTestData.PublishedArticle(product.Id, "sign-in", "Sign in", "Getting started", 1));
        // A draft article and another product's article must be excluded.
        docs.Add(DocTestData.DraftArticle(product.Id, "draft-page", "Draft", "Getting started", 5));
        docs.Add(DocTestData.PublishedArticle(ProductId.New(), "other", "Other product page", "Getting started", 0));

        var nav = await new GetProductDocsHandler(products, docs).Handle(product.Id.Value, CancellationToken.None);

        Assert.NotNull(nav);
        Assert.Equal(product.Id.Value, nav!.ProductId);
        Assert.Equal("Workflows.AI", nav.ProductName);
        Assert.Collection(
            nav.Sections,
            gettingStarted =>
            {
                Assert.Equal("Getting started", gettingStarted.Section);
                Assert.Collection(
                    gettingStarted.Articles,
                    article => Assert.Equal("Install", article.Title),
                    article => Assert.Equal("Sign in", article.Title));
            },
            advanced =>
            {
                Assert.Equal("Advanced", advanced.Section);
                var only = Assert.Single(advanced.Articles);
                Assert.Equal("Automations", only.Title);
                Assert.Equal("automations", only.Slug);
            });
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ReturnsNull()
    {
        var handler = new GetProductDocsHandler(new InMemoryProductRepository(), new InMemoryDocArticleRepository());

        var nav = await handler.Handle(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(nav);
    }

    [Fact]
    public async Task Handle_WhenProductIsDraft_ReturnsNull()
    {
        var draft = Product.CreateDraft(Slug.Create("draft-product"), "Draft", "Category", "Summary.");
        var products = new InMemoryProductRepository();
        await products.AddAsync(draft);
        var docs = new InMemoryDocArticleRepository();
        docs.Add(DocTestData.PublishedArticle(draft.Id, "page", "Page", "Getting started", 0));

        var nav = await new GetProductDocsHandler(products, docs).Handle(draft.Id.Value, CancellationToken.None);

        Assert.Null(nav);
    }

    [Fact]
    public async Task Handle_WhenProductHasNoPublishedArticles_ReturnsNull()
    {
        var product = DocTestData.PublishedProduct();
        var products = new InMemoryProductRepository();
        await products.AddAsync(product);
        var docs = new InMemoryDocArticleRepository();
        docs.Add(DocTestData.DraftArticle(product.Id, "draft-page", "Draft", "Getting started", 0));

        var nav = await new GetProductDocsHandler(products, docs).Handle(product.Id.Value, CancellationToken.None);

        Assert.Null(nav);
    }
}
