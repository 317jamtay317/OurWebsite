using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;
using ProManagerOnline.Site.Infrastructure.Persistence;
using Xunit;

namespace ProManagerOnline.Site.Infrastructure.Tests;

/// <summary>
/// Integration tests for <see cref="DocArticleRepository"/> and the EF Core mapping of the
/// <see cref="DocArticle"/> aggregate, exercised against a real (in-memory SQLite) database.
/// The repository serves the public site, so it returns only published articles.
/// </summary>
public sealed class DocArticleRepositoryTests : IDisposable
{
    public DocArticleRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<SiteDbContext>().UseSqlite(_connection).Options;

        using var context = new SiteDbContext(_options);
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetPublishedAsync_RoundTripsAPublishedArticle()
    {
        var productId = ProductId.New();
        var article = Published(productId, "getting-started", "Getting started", "Basics", "# Welcome", 0);
        await SeedAsync(article);

        await using var context = new SiteDbContext(_options);
        var loaded = await new DocArticleRepository(context)
            .GetPublishedAsync(productId, Slug.Create("getting-started"));

        Assert.NotNull(loaded);
        Assert.Equal(productId, loaded!.ProductId);
        Assert.Equal("Getting started", loaded.Title);
        Assert.Equal("Basics", loaded.Section);
        Assert.Equal("# Welcome", loaded.Body);
        Assert.Equal(0, loaded.Position);
        Assert.Equal(DocStatus.Published, loaded.Status);
    }

    [Fact]
    public async Task GetPublishedAsync_ReturnsNull_ForADraftArticle()
    {
        var productId = ProductId.New();
        var draft = DocArticle.CreateDraft(productId, Slug.Create("draft"), "Draft", "Basics", "Body.", 0);
        await SeedAsync(draft);

        await using var context = new SiteDbContext(_options);
        var loaded = await new DocArticleRepository(context).GetPublishedAsync(productId, Slug.Create("draft"));

        Assert.Null(loaded);
    }

    [Fact]
    public async Task ListPublishedByProductAsync_ReturnsOnlyThatProductsPublishedArticles_Ordered()
    {
        var productId = ProductId.New();
        var otherProductId = ProductId.New();
        await SeedAsync(
            Published(productId, "sign-in", "Sign in", "Getting started", "Body.", 1),
            Published(productId, "install", "Install", "Getting started", "Body.", 0),
            Published(productId, "create-a-quote", "Create a quote", "How-to guides", "Body.", 0),
            DocArticle.CreateDraft(productId, Slug.Create("draft"), "Draft", "Getting started", "Body.", 2),
            Published(otherProductId, "other", "Other", "Getting started", "Body.", 0));

        await using var context = new SiteDbContext(_options);
        var articles = await new DocArticleRepository(context).ListPublishedByProductAsync(productId);

        // Ordered by Section ("Getting started" < "How-to guides") then Position; draft and the
        // other product's article are excluded.
        Assert.Equal(["Install", "Sign in", "Create a quote"], articles.Select(article => article.Title));
    }

    [Fact]
    public async Task ListProductIdsWithPublishedDocsAsync_ReturnsDistinctProductIdsWithPublishedDocs()
    {
        var withDocs = ProductId.New();
        var draftOnly = ProductId.New();
        var alsoWithDocs = ProductId.New();
        await SeedAsync(
            Published(withDocs, "a", "A", "S", "Body.", 0),
            Published(withDocs, "b", "B", "S", "Body.", 1),
            DocArticle.CreateDraft(draftOnly, Slug.Create("c"), "C", "S", "Body.", 0),
            Published(alsoWithDocs, "d", "D", "S", "Body.", 0));

        await using var context = new SiteDbContext(_options);
        var ids = await new DocArticleRepository(context).ListProductIdsWithPublishedDocsAsync();

        Assert.Equal(2, ids.Count);
        Assert.Contains(withDocs, ids);
        Assert.Contains(alsoWithDocs, ids);
        Assert.DoesNotContain(draftOnly, ids);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsAnArticle_EvenWhenItIsADraft()
    {
        var draft = DocArticle.CreateDraft(ProductId.New(), Slug.Create("draft"), "Draft", "Basics", "Body.", 0);
        await SeedAsync(draft);

        await using var context = new SiteDbContext(_options);
        var loaded = await new DocArticleRepository(context).GetByIdAsync(draft.Id);

        Assert.NotNull(loaded);
        Assert.Equal(draft.Id, loaded!.Id);
        Assert.Equal(DocStatus.Draft, loaded.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNoArticleHasThatId()
    {
        await using var context = new SiteDbContext(_options);
        var loaded = await new DocArticleRepository(context).GetByIdAsync(DocArticleId.New());

        Assert.Null(loaded);
    }

    [Fact]
    public async Task ListByProductAsync_ReturnsDraftsAndPublishedForThatProduct_Ordered()
    {
        var productId = ProductId.New();
        var otherProductId = ProductId.New();
        await SeedAsync(
            Published(productId, "sign-in", "Sign in", "Getting started", "Body.", 1),
            DocArticle.CreateDraft(productId, Slug.Create("install"), "Install", "Getting started", "Body.", 0),
            Published(productId, "create-a-quote", "Create a quote", "How-to guides", "Body.", 0),
            Published(otherProductId, "other", "Other", "Getting started", "Body.", 0));

        await using var context = new SiteDbContext(_options);
        var articles = await new DocArticleRepository(context).ListByProductAsync(productId);

        // Unlike the public reader, the draft is included; ordered by Section then Position; the
        // other product's article is excluded.
        Assert.Equal(["Install", "Sign in", "Create a quote"], articles.Select(article => article.Title));
    }

    [Fact]
    public async Task SlugExistsAsync_IsTrueOnlyForTheSameProductAndSlug()
    {
        var productId = ProductId.New();
        var otherProductId = ProductId.New();
        await SeedAsync(
            DocArticle.CreateDraft(productId, Slug.Create("getting-started"), "GS", "Basics", "Body.", 0));

        await using var context = new SiteDbContext(_options);
        var repository = new DocArticleRepository(context);

        Assert.True(await repository.SlugExistsAsync(productId, Slug.Create("getting-started")));
        Assert.False(await repository.SlugExistsAsync(productId, Slug.Create("not-used")));
        Assert.False(await repository.SlugExistsAsync(otherProductId, Slug.Create("getting-started")));
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsANewArticle()
    {
        var article = DocArticle.CreateDraft(
            ProductId.New(), Slug.Create("new-page"), "New page", "Basics", "# Body", 0);

        await using (var context = new SiteDbContext(_options))
        {
            var repository = new DocArticleRepository(context);
            await repository.AddAsync(article);
            await repository.SaveChangesAsync();
        }

        await using var verify = new SiteDbContext(_options);
        var loaded = await new DocArticleRepository(verify).GetByIdAsync(article.Id);
        Assert.NotNull(loaded);
        Assert.Equal("New page", loaded!.Title);
        Assert.Equal(DocStatus.Draft, loaded.Status);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsEditsAndPublicationOfATrackedArticle()
    {
        var draft = DocArticle.CreateDraft(
            ProductId.New(), Slug.Create("draft"), "Draft title", "Basics", "Old body.", 0);
        await SeedAsync(draft);

        await using (var context = new SiteDbContext(_options))
        {
            var repository = new DocArticleRepository(context);
            var loaded = await repository.GetByIdAsync(draft.Id);
            loaded!.UpdateContent("New title", "Advanced", "New body.");
            loaded.Reposition(2);
            loaded.Publish();
            await repository.SaveChangesAsync();
        }

        await using var verify = new SiteDbContext(_options);
        var saved = await new DocArticleRepository(verify).GetByIdAsync(draft.Id);
        Assert.Equal("New title", saved!.Title);
        Assert.Equal("Advanced", saved.Section);
        Assert.Equal("New body.", saved.Body);
        Assert.Equal(2, saved.Position);
        Assert.Equal(DocStatus.Published, saved.Status);
    }

    public void Dispose() => _connection.Dispose();

    private static DocArticle Published(
        ProductId productId, string slug, string title, string section, string body, int position)
    {
        var article = DocArticle.CreateDraft(productId, Slug.Create(slug), title, section, body, position);
        article.Publish();
        return article;
    }

    private async Task SeedAsync(params DocArticle[] articles)
    {
        await using var context = new SiteDbContext(_options);
        context.DocArticles.AddRange(articles);
        await context.SaveChangesAsync();
    }

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SiteDbContext> _options;
}
