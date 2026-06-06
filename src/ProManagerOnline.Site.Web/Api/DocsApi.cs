using ProManagerOnline.Site.Contracts;

namespace ProManagerOnline.Site.Web.Api;

/// <summary>JSON endpoints for the documentation admin, consumed by the WebAssembly client.</summary>
public static class DocsApi
{
    /// <summary>Maps the <c>/api/docs</c> endpoints over <see cref="IDocsAdminApi"/>.</summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The same route builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapDocsAdminApi(this IEndpointRouteBuilder app)
    {
        // The whole documentation admin API requires an authenticated admin (cookie auth). Antiforgery
        // is disabled because the WebAssembly client calls these as a JSON/multipart API rather than
        // posting an antiforgery-tokened form (the screenshot upload posts an IFormFile).
        var group = app.MapGroup("/api/docs").RequireAuthorization().DisableAntiforgery();

        group.MapGet("/products", (IDocsAdminApi api, CancellationToken ct)
            => Run(() => api.GetPublishedProductsAsync(ct)));

        group.MapGet("/products/{productId:guid}/articles", (Guid productId, IDocsAdminApi api, CancellationToken ct)
            => Run(() => api.GetArticlesAsync(productId, ct)));

        group.MapGet("/articles/{id:guid}", async (Guid id, IDocsAdminApi api, CancellationToken ct) =>
        {
            try
            {
                var article = await api.GetArticleAsync(id, ct);
                return article is null ? Results.NotFound() : Results.Ok(article);
            }
            catch (DocsAdminException exception)
            {
                return ToResult(exception);
            }
        });

        group.MapPost("/articles", (CreateDocArticleRequest request, IDocsAdminApi api, CancellationToken ct)
            => Run(() => api.CreateArticleAsync(request, ct)));

        group.MapPut("/articles/{id:guid}", (Guid id, UpdateDocArticleRequest request, IDocsAdminApi api, CancellationToken ct)
            => Run(() => api.UpdateArticleAsync(id, request, ct)));

        group.MapPost("/articles/{id:guid}/publish", (Guid id, IDocsAdminApi api, CancellationToken ct)
            => Run(() => api.SetArticleStatusAsync(id, true, ct)));

        group.MapPost("/articles/{id:guid}/unpublish", (Guid id, IDocsAdminApi api, CancellationToken ct)
            => Run(() => api.SetArticleStatusAsync(id, false, ct)));

        group.MapPost("/media", (Guid productId, IFormFile file, IDocsAdminApi api, CancellationToken ct)
            => Run(async () =>
            {
                await using var stream = file.OpenReadStream();
                return await api.UploadScreenshotAsync(productId, file.FileName, stream, ct);
            }));

        return app;
    }

    private static async Task<IResult> Run(Func<Task> action)
    {
        try
        {
            await action();
            return Results.NoContent();
        }
        catch (DocsAdminException exception)
        {
            return ToResult(exception);
        }
    }

    private static async Task<IResult> Run<T>(Func<Task<T>> action)
    {
        try
        {
            return Results.Ok(await action());
        }
        catch (DocsAdminException exception)
        {
            return ToResult(exception);
        }
    }

    private static IResult ToResult(DocsAdminException exception) => exception.Kind switch
    {
        DocsAdminError.NotFound => Results.NotFound(exception.Message),
        DocsAdminError.Conflict => Results.Conflict(exception.Message),
        _ => Results.BadRequest(exception.Message),
    };
}
