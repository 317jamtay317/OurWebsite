using System.Net;
using System.Net.Http.Json;
using ProManagerOnline.Site.Contracts;

namespace ProManagerOnline.Site.Web.Client;

/// <summary>
/// WebAssembly <see cref="IAboutPageAdminApi"/> that calls the server's <c>/api/about</c> endpoints
/// over HTTP. Non-success responses are translated to <see cref="AboutPageAdminException"/>.
/// </summary>
/// <param name="http">The HTTP client, configured with the host's base address.</param>
public sealed class HttpClientAboutPageAdminApi(HttpClient http) : IAboutPageAdminApi
{
    private const string Root = "api/about";

    /// <inheritdoc />
    public async Task<AboutPageContent?> GetAsync(CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync(Root, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<AboutPageContent>(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveAsync(UpdateAboutPageRequest request, CancellationToken cancellationToken = default)
        => await EnsureSuccessAsync(await http.PutAsJsonAsync(Root, request, cancellationToken));

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new AboutPageAdminException(AboutPageAdminError.Invalid, await ReadMessageAsync(response));
    }

    private static async Task<string> ReadMessageAsync(HttpResponseMessage response)
    {
        try
        {
            // The API returns the failure message as a JSON string body.
            var message = await response.Content.ReadFromJsonAsync<string>();
            if (!string.IsNullOrWhiteSpace(message))
            {
                return message;
            }
        }
        catch
        {
            // Fall through to the raw body / reason phrase.
        }

        var raw = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(raw) ? response.ReasonPhrase ?? "Request failed." : raw;
    }
}
