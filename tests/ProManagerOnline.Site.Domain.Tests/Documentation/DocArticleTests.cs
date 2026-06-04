using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Domain.Tests.Documentation;

/// <summary>
/// Behaviour of the <see cref="DocArticle"/> aggregate root: a documentation article
/// that belongs to a product, grouped into a section and ordered within it.
/// </summary>
public class DocArticleTests
{
    private static DocArticle NewDraft(ProductId? productId = null) =>
        DocArticle.CreateDraft(
            productId ?? ProductId.New(),
            Slug.Create("create-and-send-a-quote"),
            title: "Create and send a quote",
            section: "How-to guides",
            body: "Go to Sales > Quotes and choose New quote.",
            position: 1);

    [Fact]
    public void CreateDraft_StartsAsADraftWithGivenDetails()
    {
        var productId = ProductId.New();

        var article = NewDraft(productId);

        Assert.Equal(DocStatus.Draft, article.Status);
        Assert.Equal(productId, article.ProductId);
        Assert.Equal("create-and-send-a-quote", article.Slug.Value);
        Assert.Equal("Create and send a quote", article.Title);
        Assert.Equal("How-to guides", article.Section);
        Assert.Equal("Go to Sales > Quotes and choose New quote.", article.Body);
        Assert.Equal(1, article.Position);
    }

    [Theory]
    [InlineData("", "How-to guides", "Body")]
    [InlineData("Title", "", "Body")]
    [InlineData("Title", "Section", "")]
    public void CreateDraft_GivenBlankTitleSectionOrBody_ThrowsDomainException(string title, string section, string body)
    {
        Assert.Throws<DomainException>(() =>
            DocArticle.CreateDraft(ProductId.New(), Slug.Create("a-slug"), title, section, body, position: 1));
    }

    [Fact]
    public void CreateDraft_GivenNegativePosition_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            DocArticle.CreateDraft(ProductId.New(), Slug.Create("a-slug"), "Title", "Section", "Body", position: -1));
    }

    [Fact]
    public void Publish_MakesItPublished()
    {
        var article = NewDraft();

        article.Publish();

        Assert.Equal(DocStatus.Published, article.Status);
    }

    [Fact]
    public void Unpublish_ReturnsItToDraft()
    {
        var article = NewDraft();
        article.Publish();

        article.Unpublish();

        Assert.Equal(DocStatus.Draft, article.Status);
    }

    [Fact]
    public void UpdateContent_ChangesTitleSectionAndBody()
    {
        var article = NewDraft();

        article.UpdateContent("New title", "Getting started", "New body.");

        Assert.Equal("New title", article.Title);
        Assert.Equal("Getting started", article.Section);
        Assert.Equal("New body.", article.Body);
    }

    [Fact]
    public void Reposition_ChangesThePosition()
    {
        var article = NewDraft();

        article.Reposition(5);

        Assert.Equal(5, article.Position);
    }
}
