using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProManagerOnline.Site.Domain.Content;
using ProManagerOnline.Site.Infrastructure.Persistence;
using Xunit;

namespace ProManagerOnline.Site.Infrastructure.Tests;

/// <summary>
/// Integration tests for <see cref="AboutPageRepository"/> and the EF Core mapping of the singleton
/// <see cref="AboutPage"/> aggregate, exercised against a real (in-memory SQLite) database.
/// </summary>
public sealed class AboutPageRepositoryTests : IDisposable
{
    public AboutPageRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<SiteDbContext>().UseSqlite(_connection).Options;

        using var context = new SiteDbContext(_options);
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenThePageHasNotBeenCreated()
    {
        await using var context = new SiteDbContext(_options);

        Assert.Null(await new AboutPageRepository(context).GetAsync());
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsThePage()
    {
        var page = AboutPage.Create("About us", "# We build software");

        await using (var context = new SiteDbContext(_options))
        {
            var repository = new AboutPageRepository(context);
            await repository.AddAsync(page);
            await repository.SaveChangesAsync();
        }

        await using var verify = new SiteDbContext(_options);
        var loaded = await new AboutPageRepository(verify).GetAsync();
        Assert.NotNull(loaded);
        Assert.Equal(AboutPageId.Single, loaded!.Id);
        Assert.Equal("About us", loaded.Title);
        Assert.Equal("# We build software", loaded.Body);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsEditsToATrackedPage()
    {
        await using (var seed = new SiteDbContext(_options))
        {
            seed.AboutPages.Add(AboutPage.Create("Old title", "Old body."));
            await seed.SaveChangesAsync();
        }

        await using (var context = new SiteDbContext(_options))
        {
            var repository = new AboutPageRepository(context);
            var loaded = await repository.GetAsync();
            loaded!.UpdateContent("New title", "New body.");
            await repository.SaveChangesAsync();
        }

        await using var verify = new SiteDbContext(_options);
        var saved = await new AboutPageRepository(verify).GetAsync();
        Assert.Equal("New title", saved!.Title);
        Assert.Equal("New body.", saved.Body);
    }

    public void Dispose() => _connection.Dispose();

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SiteDbContext> _options;
}
