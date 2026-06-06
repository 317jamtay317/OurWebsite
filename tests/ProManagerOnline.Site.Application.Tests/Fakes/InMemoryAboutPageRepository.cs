using ProManagerOnline.Site.Domain.Content;

namespace ProManagerOnline.Site.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IAboutPageRepository"/> used as a test double for application handlers,
/// so the About-page use cases can be tested without a database. Holds at most one page, mirroring
/// the singleton nature of the production store.
/// </summary>
internal sealed class InMemoryAboutPageRepository : IAboutPageRepository
{
    /// <summary>The number of times <see cref="SaveChangesAsync"/> has been called, for assertions.</summary>
    public int SaveChangesCount { get; private set; }

    /// <summary>Seeds the stored page directly, for arranging tests.</summary>
    /// <param name="page">The page to store.</param>
    public void Set(AboutPage page) => _page = page;

    public Task<AboutPage?> GetAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_page);

    public Task AddAsync(AboutPage page, CancellationToken cancellationToken = default)
    {
        _page = page;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }

    private AboutPage? _page;
}
