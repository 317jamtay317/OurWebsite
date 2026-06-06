namespace ProManagerOnline.Site.Contracts;

/// <summary>
/// The About-page administration API consumed by the Blazor admin components. The same interface
/// has a server implementation (calling the application handlers in process) and a WebAssembly
/// implementation (calling the JSON API over HTTP), so Interactive Auto components work either way.
/// Operations throw <see cref="AboutPageAdminException"/> on failure.
/// </summary>
public interface IAboutPageAdminApi
{
    /// <summary>Loads the current About-page content, or <see langword="null"/> when it has not been set yet.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<AboutPageContent?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Saves new content for the About page, creating it if it does not exist yet.</summary>
    /// <param name="request">The new title and body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SaveAsync(UpdateAboutPageRequest request, CancellationToken cancellationToken = default);
}
