using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Documentation;

/// <summary>
/// Behaviour of the <see cref="ListProductDocArticlesHandler"/> admin query: it lists every one
/// of a product's articles — drafts and published — as ordered rows, so the authoring admin can
/// show work in progress alongside live content.
/// </summary>
public class ListProductDocArticlesHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsDraftsAndPublishedForTheProduct_OrderedWithStatus()
    {
        var productId = ProductId.New();
        var otherProduct = ProductId.New();
        var docs = new InMemoryDocArticleRepository();
        docs.Add(DocTestData.PublishedArticle(productId, "sign-in", "Sign in", "Getting started", 1));
        docs.Add(DocTestData.DraftArticle(productId, "install", "Install", "Getting started", 0));
        docs.Add(DocTestData.PublishedArticle(productId, "create-a-quote", "Create a quote", "How-to guides", 0));
        docs.Add(DocTestData.PublishedArticle(otherProduct, "other", "Other", "Getting started", 0));

        var rows = await new ListProductDocArticlesHandler(docs).Handle(productId.Value, CancellationToken.None);

        // Ordered by Section then Position; the other product's article is excluded.
        Assert.Equal(["Install", "Sign in", "Create a quote"], rows.Select(row => row.Title));
        Assert.Equal(DocStatus.Draft, rows[0].Status);
        Assert.Equal(DocStatus.Published, rows[1].Status);
        Assert.Equal("install", rows[0].Slug);
    }

    [Fact]
    public async Task Handle_WhenProductHasNoArticles_ReturnsEmpty()
    {
        var rows = await new ListProductDocArticlesHandler(new InMemoryDocArticleRepository())
            .Handle(ProductId.New().Value, CancellationToken.None);

        Assert.Empty(rows);
    }
}
