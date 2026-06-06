using ProManagerOnline.Site.Domain.Content;
using ProManagerOnline.Site.Domain.Exceptions;
using Xunit;

namespace ProManagerOnline.Site.Domain.Tests.Content;

/// <summary>
/// Behaviour of the <see cref="AboutPage"/> aggregate root: the site's single, editable
/// "About" content page, authored as a title and a Markdown body.
/// </summary>
public class AboutPageTests
{
    [Fact]
    public void Create_SetsTheTitleAndBody()
    {
        var page = AboutPage.Create("About us", "We build software.");

        Assert.Equal("About us", page.Title);
        Assert.Equal("We build software.", page.Body);
    }

    [Fact]
    public void Create_GivesThePageTheWellKnownSingletonIdentity()
    {
        var page = AboutPage.Create("About us", "We build software.");

        // There is only ever one About page, so every instance shares the same identity.
        Assert.Equal(AboutPageId.Single, page.Id);
    }

    [Fact]
    public void Create_TrimsSurroundingWhitespace()
    {
        var page = AboutPage.Create("  About us  ", "  We build software.  ");

        Assert.Equal("About us", page.Title);
        Assert.Equal("We build software.", page.Body);
    }

    [Theory]
    [InlineData("", "Body")]
    [InlineData("   ", "Body")]
    [InlineData("Title", "")]
    [InlineData("Title", "   ")]
    public void Create_GivenBlankTitleOrBody_ThrowsDomainException(string title, string body)
    {
        Assert.Throws<DomainException>(() => AboutPage.Create(title, body));
    }

    [Fact]
    public void UpdateContent_ChangesTheTitleAndBody()
    {
        var page = AboutPage.Create("About us", "Old body.");

        page.UpdateContent("Our story", "New body.");

        Assert.Equal("Our story", page.Title);
        Assert.Equal("New body.", page.Body);
    }

    [Fact]
    public void UpdateContent_TrimsSurroundingWhitespace()
    {
        var page = AboutPage.Create("About us", "Old body.");

        page.UpdateContent("  Our story  ", "  New body.  ");

        Assert.Equal("Our story", page.Title);
        Assert.Equal("New body.", page.Body);
    }

    [Theory]
    [InlineData("", "Body")]
    [InlineData("Title", "")]
    public void UpdateContent_GivenBlankTitleOrBody_ThrowsDomainException(string title, string body)
    {
        var page = AboutPage.Create("About us", "Body.");

        Assert.Throws<DomainException>(() => page.UpdateContent(title, body));
    }
}
