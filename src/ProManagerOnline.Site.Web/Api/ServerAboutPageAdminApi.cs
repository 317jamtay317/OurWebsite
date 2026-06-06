using ProManagerOnline.Site.Application.Content;
using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Domain.Exceptions;

namespace ProManagerOnline.Site.Web.Api;

/// <summary>
/// Server-side <see cref="IAboutPageAdminApi"/> that calls the application handlers in process. Used
/// when Interactive Auto components render on the server, and by the JSON API. Domain exceptions are
/// translated to <see cref="AboutPageAdminException"/> so callers handle them uniformly.
/// </summary>
/// <param name="getPage">Loads the About page for editing.</param>
/// <param name="savePage">Creates or updates the About page.</param>
public sealed class ServerAboutPageAdminApi(
    GetAboutPageForEditHandler getPage,
    SaveAboutPageHandler savePage) : IAboutPageAdminApi
{
    /// <inheritdoc />
    public Task<AboutPageContent?> GetAsync(CancellationToken cancellationToken = default)
        => Guard(() => getPage.Handle(cancellationToken));

    /// <inheritdoc />
    public Task SaveAsync(UpdateAboutPageRequest request, CancellationToken cancellationToken = default)
        => Guard(() => savePage.Handle(new SaveAboutPageCommand(request.Title, request.Body), cancellationToken));

    private static async Task Guard(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (DomainException exception)
        {
            throw new AboutPageAdminException(AboutPageAdminError.Invalid, exception.Message);
        }
    }

    private static async Task<T> Guard<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (DomainException exception)
        {
            throw new AboutPageAdminException(AboutPageAdminError.Invalid, exception.Message);
        }
    }
}
