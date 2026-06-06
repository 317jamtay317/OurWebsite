using System.Net;
using System.Net.Http.Json;
using ProManagerOnline.Site.Contracts;

namespace ProManagerOnline.Site.Web.Client;

/// <summary>
/// WebAssembly <see cref="IDocsAdminApi"/> that calls the server's <c>/api/docs</c> endpoints over
/// HTTP. Non-success responses are translated to <see cref="DocsAdminException"/>.
/// </summary>
/// <param name="http">The HTTP client, configured with the host's base address.</param>
public sealed class HttpClientDocsAdminApi(HttpClient http) : IDocsAdminApi
{
    private const string Root = "api/docs";

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductOption>> GetPublishedProductsAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<ProductOption>>($"{Root}/products", cancellationToken) ?? [];

    /// <inheritdoc />
    public async Task<IReadOnlyList<DocArticleRow>> GetArticlesAsync(Guid productId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<DocArticleRow>>(
            $"{Root}/products/{productId}/articles", cancellationToken) ?? [];

    /// <inheritdoc />
    public async Task<DocArticleEdit?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"{Root}/articles/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<DocArticleEdit>(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateArticleAsync(CreateDocArticleRequest request, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync($"{Root}/articles", request, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateArticleAsync(Guid id, UpdateDocArticleRequest request, CancellationToken cancellationToken = default)
        => SendAsync(() => http.PutAsJsonAsync($"{Root}/articles/{id}", request, cancellationToken));

    /// <inheritdoc />
    public Task SetArticleStatusAsync(Guid id, bool publish, CancellationToken cancellationToken = default)
        => SendAsync(() => http.PostAsync(
            $"{Root}/articles/{id}/{(publish ? "publish" : "unpublish")}", null, cancellationToken));

    /// <inheritdoc />
    public async Task<string> UploadScreenshotAsync(
        Guid productId, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(content);
        form.Add(fileContent, "file", fileName);

        var response = await http.PostAsync($"{Root}/media?productId={productId}", form, cancellationToken);
        await EnsureSuccessAsync(response);

        return await response.Content.ReadFromJsonAsync<string>(cancellationToken) ?? "";
    }

    private static async Task SendAsync(Func<Task<HttpResponseMessage>> action)
        => await EnsureSuccessAsync(await action());

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await ReadMessageAsync(response);
        var kind = response.StatusCode switch
        {
            HttpStatusCode.NotFound => DocsAdminError.NotFound,
            HttpStatusCode.Conflict => DocsAdminError.Conflict,
            _ => DocsAdminError.Invalid,
        };

        throw new DocsAdminException(kind, message);
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
