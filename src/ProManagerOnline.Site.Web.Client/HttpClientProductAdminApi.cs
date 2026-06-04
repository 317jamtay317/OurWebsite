using System.Net;
using System.Net.Http.Json;
using ProManagerOnline.Site.Contracts;

namespace ProManagerOnline.Site.Web.Client;

/// <summary>
/// WebAssembly <see cref="IProductAdminApi"/> that calls the server's <c>/api/products</c> endpoints
/// over HTTP. Non-success responses are translated to <see cref="ProductAdminException"/>.
/// </summary>
public sealed class HttpClientProductAdminApi(HttpClient http) : IProductAdminApi
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItem>> ListAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<ProductListItem>>("api/products", cancellationToken) ?? [];

    /// <inheritdoc />
    public async Task<ProductDetail?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/products/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<ProductDetail>(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/products", request, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateDetailsAsync(Guid id, UpdateProductDetailsRequest request, CancellationToken cancellationToken = default)
        => SendAsync(() => http.PutAsJsonAsync($"api/products/{id}/details", request, cancellationToken));

    /// <inheritdoc />
    public Task SetPricingAsync(Guid id, SetProductPricingRequest request, CancellationToken cancellationToken = default)
        => SendAsync(() => http.PutAsJsonAsync($"api/products/{id}/pricing", request, cancellationToken));

    /// <inheritdoc />
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => SendAsync(() => http.DeleteAsync($"api/products/{id}", cancellationToken));

    /// <inheritdoc />
    public Task PublishAsync(Guid id, CancellationToken cancellationToken = default)
        => SendAsync(() => http.PostAsync($"api/products/{id}/publish", null, cancellationToken));

    /// <inheritdoc />
    public Task UnpublishAsync(Guid id, CancellationToken cancellationToken = default)
        => SendAsync(() => http.PostAsync($"api/products/{id}/unpublish", null, cancellationToken));

    /// <inheritdoc />
    public Task AddPlanAsync(Guid id, AddPlanRequest request, CancellationToken cancellationToken = default)
        => SendAsync(() => http.PostAsJsonAsync($"api/products/{id}/plans", request, cancellationToken));

    /// <inheritdoc />
    public Task UpdatePlanAsync(Guid id, Guid planId, UpdatePlanRequest request, CancellationToken cancellationToken = default)
        => SendAsync(() => http.PutAsJsonAsync($"api/products/{id}/plans/{planId}", request, cancellationToken));

    /// <inheritdoc />
    public Task RemovePlanAsync(Guid id, Guid planId, CancellationToken cancellationToken = default)
        => SendAsync(() => http.DeleteAsync($"api/products/{id}/plans/{planId}", cancellationToken));

    /// <inheritdoc />
    public Task FeaturePlanAsync(Guid id, Guid planId, CancellationToken cancellationToken = default)
        => SendAsync(() => http.PostAsync($"api/products/{id}/plans/{planId}/feature", null, cancellationToken));

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
            HttpStatusCode.NotFound => ProductAdminError.NotFound,
            HttpStatusCode.Conflict => ProductAdminError.Conflict,
            _ => ProductAdminError.Invalid,
        };

        throw new ProductAdminException(kind, message);
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
