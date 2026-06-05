using System.Net;
using System.Net.Http.Json;
using ProManagerOnline.Site.Web.Client.Contracts;

namespace ProManagerOnline.Site.Web.Client.Services;

/// <summary>
/// <see cref="IDocsAdminApi"/> implementation used when the admin runs in WebAssembly: it calls
/// the server's JSON API at <c>/api/admin/docs…</c> with <see cref="HttpClient"/>.
/// </summary>
/// <param name="http">The HTTP client, configured with the host's base address.</param>
public sealed class HttpDocsAdminApi(HttpClient http) : IDocsAdminApi
{
    private const string ArticlesRoot = "api/admin/docs";

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductOption>> GetPublishedProductsAsync(
        CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<IReadOnlyList<ProductOption>>(
            $"{ArticlesRoot}/products", cancellationToken) ?? [];

    /// <inheritdoc />
    public async Task<IReadOnlyList<DocArticleRow>> GetArticlesAsync(
        Guid productId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<IReadOnlyList<DocArticleRow>>(
            $"{ArticlesRoot}/products/{productId}/articles", cancellationToken) ?? [];

    /// <inheritdoc />
    public async Task<DocArticleEdit?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"{ArticlesRoot}/articles/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<DocArticleEdit>(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateArticleAsync(
        CreateDocArticleRequest request, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync($"{ArticlesRoot}/articles", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>(cancellationToken);
        return created!.Id;
    }

    /// <inheritdoc />
    public async Task UpdateArticleAsync(
        Guid id, UpdateDocArticleRequest request, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"{ArticlesRoot}/articles/{id}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetArticleStatusAsync(Guid id, bool publish, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync(
            $"{ArticlesRoot}/articles/{id}/status", new SetStatusRequest(publish), cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> UploadScreenshotAsync(
        Guid productId, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(content);
        form.Add(fileContent, "file", fileName);

        var response = await http.PostAsync(
            $"{ArticlesRoot}-media?productId={productId}", form, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<UploadResponse>(cancellationToken);
        return result!.Url;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await TryReadMessageAsync(response, cancellationToken);
        throw new DocsAdminApiException(message ?? $"The request failed with status {(int)response.StatusCode}.");
    }

    private static async Task<string?> TryReadMessageAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken);
            return error?.Message;
        }
        catch (Exception)
        {
            // The body was not the expected JSON error shape; fall back to a generic message.
            return null;
        }
    }
}
