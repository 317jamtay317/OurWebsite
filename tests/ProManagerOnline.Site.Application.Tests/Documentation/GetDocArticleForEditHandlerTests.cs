using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Documentation;

/// <summary>
/// Behaviour of the <see cref="GetDocArticleForEditHandler"/> admin query: it loads a single
/// article — draft or published — by id, with every field the editor form needs.
/// </summary>
public class GetDocArticleForEditHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTheArticleForEditing()
    {
        var productId = ProductId.New();
        var docs = new InMemoryDocArticleRepository();
        var article = DocTestData.PublishedArticle(
            productId, "getting-started", "Getting started", "Basics", 2, "# Hi");
        docs.Add(article);

        var dto = await new GetDocArticleForEditHandler(docs).Handle(article.Id.Value, CancellationToken.None);

        Assert.NotNull(dto);
        Assert.Equal(article.Id.Value, dto!.Id);
        Assert.Equal(productId.Value, dto.ProductId);
        Assert.Equal("getting-started", dto.Slug);
        Assert.Equal("Getting started", dto.Title);
        Assert.Equal("Basics", dto.Section);
        Assert.Equal("# Hi", dto.Body);
        Assert.Equal(2, dto.Position);
        Assert.Equal(DocStatus.Published, dto.Status);
    }

    [Fact]
    public async Task Handle_WhenArticleNotFound_ReturnsNull()
    {
        var dto = await new GetDocArticleForEditHandler(new InMemoryDocArticleRepository())
            .Handle(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(dto);
    }
}
