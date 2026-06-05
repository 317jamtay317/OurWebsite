using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Documentation;

/// <summary>
/// Behaviour of the <see cref="UpdateDocArticleHandler"/> use case: it edits an article's
/// content and position without altering its publication status.
/// </summary>
public class UpdateDocArticleHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesContentAndPosition_LeavingStatusUnchanged()
    {
        var docs = new InMemoryDocArticleRepository();
        var article = DocTestData.PublishedArticle(
            ProductId.New(), "getting-started", "Old title", "Basics", 0, "Old body.");
        docs.Add(article);
        var handler = new UpdateDocArticleHandler(docs);

        await handler.Handle(
            new UpdateDocArticleCommand(article.Id.Value, "New title", "Advanced", "New body.", 3),
            CancellationToken.None);

        var saved = await docs.GetByIdAsync(article.Id);
        Assert.Equal("New title", saved!.Title);
        Assert.Equal("Advanced", saved.Section);
        Assert.Equal("New body.", saved.Body);
        Assert.Equal(3, saved.Position);
        Assert.Equal(DocStatus.Published, saved.Status);
        Assert.Equal(1, docs.SaveChangesCount);
    }

    [Fact]
    public async Task Handle_WhenArticleNotFound_ThrowsNotFound()
    {
        var handler = new UpdateDocArticleHandler(new InMemoryDocArticleRepository());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new UpdateDocArticleCommand(Guid.NewGuid(), "Title", "Section", "Body.", 0),
            CancellationToken.None));
    }
}
