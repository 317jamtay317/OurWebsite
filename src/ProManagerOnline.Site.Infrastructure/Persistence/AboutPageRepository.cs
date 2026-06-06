using Microsoft.EntityFrameworkCore;
using ProManagerOnline.Site.Domain.Content;

namespace ProManagerOnline.Site.Infrastructure.Persistence;

/// <summary>EF Core implementation of <see cref="IAboutPageRepository"/>.</summary>
/// <param name="context">The site database context.</param>
public sealed class AboutPageRepository(SiteDbContext context) : IAboutPageRepository
{
    /// <inheritdoc />
    public async Task<AboutPage?> GetAsync(CancellationToken cancellationToken = default)
        => await context.AboutPages.FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public Task AddAsync(AboutPage page, CancellationToken cancellationToken = default)
    {
        context.AboutPages.Add(page);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
