using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Documentation;

/// <summary>
/// Behaviour of the <see cref="SetDocArticleStatusHandler"/> use case: it publishes a draft so
/// it appears on the public site, and unpublishes an article to hide it again.
/// </summary>
public class SetDocArticleStatusHandlerTests
{
    [Fact]
    public async Task Handle_WhenPublishing_MarksADraftAsPublished()
    {
        var docs = new InMemoryDocArticleRepository();
        var draft = DocTestData.DraftArticle(ProductId.New(), "getting-started", "Getting started", "Basics", 0);
        docs.Add(draft);
        var handler = new SetDocArticleStatusHandler(docs);

        await handler.Handle(new SetDocArticleStatusCommand(draft.Id.Value, Publish: true), CancellationToken.None);

        var saved = await docs.GetByIdAsync(draft.Id);
        Assert.Equal(DocStatus.Published, saved!.Status);
        Assert.Equal(1, docs.SaveChangesCount);
    }

    [Fact]
    public async Task Handle_WhenUnpublishing_ReturnsAPublishedArticleToDraft()
    {
        var docs = new InMemoryDocArticleRepository();
        var article = DocTestData.PublishedArticle(ProductId.New(), "getting-started", "Getting started", "Basics", 0);
        docs.Add(article);
        var handler = new SetDocArticleStatusHandler(docs);

        await handler.Handle(new SetDocArticleStatusCommand(article.Id.Value, Publish: false), CancellationToken.None);

        var saved = await docs.GetByIdAsync(article.Id);
        Assert.Equal(DocStatus.Draft, saved!.Status);
    }

    [Fact]
    public async Task Handle_WhenArticleNotFound_ThrowsNotFound()
    {
        var handler = new SetDocArticleStatusHandler(new InMemoryDocArticleRepository());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new SetDocArticleStatusCommand(Guid.NewGuid(), Publish: true), CancellationToken.None));
    }
}
