using ProManagerOnline.Site.Application.Content;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Content;
using ProManagerOnline.Site.Domain.Exceptions;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Content;

/// <summary>
/// Behaviour of <see cref="SaveAboutPageHandler"/>, which upserts the About page: it creates the
/// page the first time it is saved and edits it thereafter.
/// </summary>
public class SaveAboutPageHandlerTests
{
    [Fact]
    public async Task Handle_WhenThePageDoesNotExistYet_CreatesItAndSaves()
    {
        var repository = new InMemoryAboutPageRepository();
        var handler = new SaveAboutPageHandler(repository);

        await handler.Handle(new SaveAboutPageCommand("About us", "We build software."));

        var page = await repository.GetAsync();
        Assert.NotNull(page);
        Assert.Equal("About us", page!.Title);
        Assert.Equal("We build software.", page.Body);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task Handle_WhenThePageExists_UpdatesItInPlaceAndSaves()
    {
        var repository = new InMemoryAboutPageRepository();
        repository.Set(AboutPage.Create("Old title", "Old body."));
        var handler = new SaveAboutPageHandler(repository);

        await handler.Handle(new SaveAboutPageCommand("New title", "New body."));

        var page = await repository.GetAsync();
        Assert.Equal("New title", page!.Title);
        Assert.Equal("New body.", page.Body);
        Assert.Equal(AboutPageId.Single, page.Id);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task Handle_GivenBlankContent_ThrowsDomainExceptionAndDoesNotSave()
    {
        var repository = new InMemoryAboutPageRepository();
        var handler = new SaveAboutPageHandler(repository);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new SaveAboutPageCommand("", "Body.")));
        Assert.Equal(0, repository.SaveChangesCount);
    }
}
