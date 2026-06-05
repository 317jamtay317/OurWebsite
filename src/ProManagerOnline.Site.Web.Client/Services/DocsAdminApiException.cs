namespace ProManagerOnline.Site.Web.Client.Services;

/// <summary>
/// Thrown by <see cref="HttpDocsAdminApi"/> when the admin API returns an error, carrying the
/// server's message so the editor can show it to the author (for example "slug already exists").
/// </summary>
public sealed class DocsAdminApiException : Exception
{
    /// <summary>Initialises a new <see cref="DocsAdminApiException"/>.</summary>
    /// <param name="message">The error message to show the author.</param>
    public DocsAdminApiException(string message)
        : base(message)
    {
    }
}
