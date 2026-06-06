using ProManagerOnline.Site.Domain.Content;
using ProManagerOnline.Site.Domain.Exceptions;

namespace ProManagerOnline.Site.Application.Content;

/// <summary>Input for saving the About page's content.</summary>
/// <param name="Title">The new heading.</param>
/// <param name="Body">The new Markdown body.</param>
public sealed record SaveAboutPageCommand(string Title, string Body);

/// <summary>
/// Saves the About page's content, upserting it: the page is created the first time it is saved and
/// edited in place thereafter, so the admin always has a page to work with.
/// </summary>
/// <param name="pages">The About-page repository.</param>
public sealed class SaveAboutPageHandler(IAboutPageRepository pages)
{
    /// <summary>Creates or updates the About page with the given content.</summary>
    /// <param name="command">The new title and body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="DomainException">Thrown when the title or body is blank.</exception>
    public async Task Handle(SaveAboutPageCommand command, CancellationToken cancellationToken = default)
    {
        var page = await pages.GetAsync(cancellationToken);

        if (page is null)
        {
            await pages.AddAsync(AboutPage.Create(command.Title, command.Body), cancellationToken);
        }
        else
        {
            page.UpdateContent(command.Title, command.Body);
        }

        await pages.SaveChangesAsync(cancellationToken);
    }
}
