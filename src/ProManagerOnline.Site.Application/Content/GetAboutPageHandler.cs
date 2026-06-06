using ProManagerOnline.Site.Domain.Content;

namespace ProManagerOnline.Site.Application.Content;

/// <summary>Loads the About page's content as the public read model for the <c>/about</c> page.</summary>
/// <param name="pages">The About-page repository.</param>
public sealed class GetAboutPageHandler(IAboutPageRepository pages)
{
    /// <summary>Loads the About page.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The page's content, or <see langword="null"/> when it has not been created yet.</returns>
    public async Task<AboutPageDto?> Handle(CancellationToken cancellationToken = default)
    {
        var page = await pages.GetAsync(cancellationToken);

        return page is null ? null : new AboutPageDto(page.Title, page.Body);
    }
}
