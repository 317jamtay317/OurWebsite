using ProManagerOnline.Site.Application.Content;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Content;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Content;

/// <summary>
/// Behaviour of <see cref="GetAboutPageHandler"/>, the public read model used to render the
/// <c>/about</c> page.
/// </summary>
public class GetAboutPageHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTheContent_WhenThePageExists()
    {
        var repository = new InMemoryAboutPageRepository();
        repository.Set(AboutPage.Create("About us", "We build software."));
        var handler = new GetAboutPageHandler(repository);

        var result = await handler.Handle();

        Assert.NotNull(result);
        Assert.Equal("About us", result!.Title);
        Assert.Equal("We build software.", result.Body);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenThePageHasNotBeenCreated()
    {
        var handler = new GetAboutPageHandler(new InMemoryAboutPageRepository());

        Assert.Null(await handler.Handle());
    }
}
