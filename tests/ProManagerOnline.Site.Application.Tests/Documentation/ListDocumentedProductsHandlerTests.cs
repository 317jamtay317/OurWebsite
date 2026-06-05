using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Application.Tests.Fakes;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Documentation;

/// <summary>
/// Behaviour of the <see cref="ListDocumentedProductsHandler"/> query: the documentation
/// landing lists only published products that actually have published articles.
/// </summary>
public class ListDocumentedProductsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyPublishedProductsThatHavePublishedDocs()
    {
        var withDocs = DocTestData.PublishedProduct("workflows", "Workflows.AI");
        var withoutDocs = DocTestData.PublishedProduct("air-compliance", "Air Compliance");
        var draftDocsOnly = DocTestData.PublishedProduct("crm", "CRM");
        var draftProduct = Product.CreateDraft(Slug.Create("secret"), "Secret", "Category", "Summary.");

        var products = new InMemoryProductRepository();
        await products.AddAsync(withDocs);
        await products.AddAsync(withoutDocs);
        await products.AddAsync(draftDocsOnly);
        await products.AddAsync(draftProduct);

        var docs = new InMemoryDocArticleRepository();
        docs.Add(DocTestData.PublishedArticle(withDocs.Id, "install", "Install", "Getting started", 0));
        docs.Add(DocTestData.PublishedArticle(withDocs.Id, "sign-in", "Sign in", "Getting started", 1));
        docs.Add(DocTestData.DraftArticle(draftDocsOnly.Id, "draft", "Draft", "Getting started", 0));
        docs.Add(DocTestData.PublishedArticle(draftProduct.Id, "leak", "Leak", "Getting started", 0));

        var result = await new ListDocumentedProductsHandler(products, docs).Handle(CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal(withDocs.Id.Value, dto.ProductId);
        Assert.Equal("Workflows.AI", dto.ProductName);
        Assert.Equal(2, dto.ArticleCount);
    }

    [Fact]
    public async Task Handle_WhenNothingIsDocumented_ReturnsEmpty()
    {
        var products = new InMemoryProductRepository();
        await products.AddAsync(DocTestData.PublishedProduct());

        var result = await new ListDocumentedProductsHandler(products, new InMemoryDocArticleRepository())
            .Handle(CancellationToken.None);

        Assert.Empty(result);
    }
}
