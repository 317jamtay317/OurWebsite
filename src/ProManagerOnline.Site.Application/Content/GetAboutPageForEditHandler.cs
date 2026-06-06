using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Domain.Content;

namespace ProManagerOnline.Site.Application.Content;

/// <summary>
/// Loads the About page's content as the edit read model the authoring editor populates its form
/// from.
/// </summary>
/// <param name="pages">The About-page repository.</param>
public sealed class GetAboutPageForEditHandler(IAboutPageRepository pages)
{
    /// <summary>Loads the About page for editing.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The page's content, or <see langword="null"/> when it has not been created yet.</returns>
    public async Task<AboutPageContent?> Handle(CancellationToken cancellationToken = default)
    {
        var page = await pages.GetAsync(cancellationToken);

        return page is null ? null : new AboutPageContent(page.Title, page.Body);
    }
}
