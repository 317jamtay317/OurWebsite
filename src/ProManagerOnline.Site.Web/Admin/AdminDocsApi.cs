using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Web.Client.Contracts;
using ProManagerOnline.Site.Web.Client.Services;

namespace ProManagerOnline.Site.Web.Admin;

/// <summary>
/// Maps the JSON API the WebAssembly documentation admin calls. Each endpoint delegates to
/// <see cref="IDocsAdminApi"/> (resolved to the in-process server implementation) and translates
/// application exceptions to HTTP status codes. Mapped only in Development — there is no
/// authentication yet, so it must not be exposed in production.
/// </summary>
public static class AdminDocsApi
{
    /// <summary>Maps the documentation admin's <c>/api/admin/docs…</c> endpoints.</summary>
    /// <param name="app">The endpoint route builder to map onto.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapAdminDocsApi(this IEndpointRouteBuilder app)
    {
        // The browser client cannot supply an antiforgery token, and the admin is Development-only,
        // so antiforgery is disabled on these endpoints.
        var group = app.MapGroup("/api/admin/docs").DisableAntiforgery();

        group.MapGet("/products", async (IDocsAdminApi api, CancellationToken cancellationToken)
            => Results.Ok(await api.GetPublishedProductsAsync(cancellationToken)));

        group.MapGet("/products/{productId:guid}/articles",
            async (Guid productId, IDocsAdminApi api, CancellationToken cancellationToken)
                => Results.Ok(await api.GetArticlesAsync(productId, cancellationToken)));

        group.MapGet("/articles/{id:guid}", async (Guid id, IDocsAdminApi api, CancellationToken cancellationToken) =>
        {
            var article = await api.GetArticleAsync(id, cancellationToken);
            return article is null ? NotFound(id) : Results.Ok(article);
        });

        group.MapPost("/articles",
            async (CreateDocArticleRequest request, IDocsAdminApi api, CancellationToken cancellationToken) =>
            {
                try
                {
                    var id = await api.CreateArticleAsync(request, cancellationToken);
                    return Results.Ok(new CreatedResponse(id));
                }
                catch (ConflictException ex)
                {
                    return Error(ex.Message, StatusCodes.Status409Conflict);
                }
                catch (DomainException ex)
                {
                    return Error(ex.Message, StatusCodes.Status400BadRequest);
                }
            });

        group.MapPut("/articles/{id:guid}",
            async (Guid id, UpdateDocArticleRequest request, IDocsAdminApi api, CancellationToken cancellationToken) =>
            {
                try
                {
                    await api.UpdateArticleAsync(id, request, cancellationToken);
                    return Results.NoContent();
                }
                catch (NotFoundException ex)
                {
                    return Error(ex.Message, StatusCodes.Status404NotFound);
                }
                catch (DomainException ex)
                {
                    return Error(ex.Message, StatusCodes.Status400BadRequest);
                }
            });

        group.MapPost("/articles/{id:guid}/status",
            async (Guid id, SetStatusRequest request, IDocsAdminApi api, CancellationToken cancellationToken) =>
            {
                try
                {
                    await api.SetArticleStatusAsync(id, request.Publish, cancellationToken);
                    return Results.NoContent();
                }
                catch (NotFoundException ex)
                {
                    return Error(ex.Message, StatusCodes.Status404NotFound);
                }
            });

        // The media endpoint is a sibling route (/api/admin/docs-media), not part of the group.
        app.MapPost("/api/admin/docs-media",
            async (Guid productId, IFormFile file, IDocsAdminApi api, CancellationToken cancellationToken) =>
            {
                await using var stream = file.OpenReadStream();
                var url = await api.UploadScreenshotAsync(productId, file.FileName, stream, cancellationToken);
                return Results.Ok(new UploadResponse(url));
            }).DisableAntiforgery();

        return app;
    }

    private static IResult NotFound(Guid id)
        => Error($"No documentation article exists with id {id}.", StatusCodes.Status404NotFound);

    private static IResult Error(string message, int statusCode)
        => Results.Json(new ErrorResponse(message), statusCode: statusCode);
}
