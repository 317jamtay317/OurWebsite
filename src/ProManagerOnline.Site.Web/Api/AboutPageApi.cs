using ProManagerOnline.Site.Contracts;

namespace ProManagerOnline.Site.Web.Api;

/// <summary>JSON endpoints for the About-page admin, consumed by the WebAssembly client.</summary>
public static class AboutPageApi
{
    /// <summary>Maps the <c>/api/about</c> endpoints over <see cref="IAboutPageAdminApi"/>.</summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The same route builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapAboutPageAdminApi(this IEndpointRouteBuilder app)
    {
        // The About-page admin API requires an authenticated admin (cookie auth). Antiforgery is
        // disabled because the WebAssembly client calls these as a JSON API rather than posting an
        // antiforgery-tokened form, consistent with the products and documentation admin APIs.
        var group = app.MapGroup("/api/about").RequireAuthorization().DisableAntiforgery();

        group.MapGet("", async (IAboutPageAdminApi api, CancellationToken ct) =>
        {
            try
            {
                var content = await api.GetAsync(ct);
                return content is null ? Results.NoContent() : Results.Ok(content);
            }
            catch (AboutPageAdminException exception)
            {
                return ToResult(exception);
            }
        });

        group.MapPut("", async (UpdateAboutPageRequest request, IAboutPageAdminApi api, CancellationToken ct) =>
        {
            try
            {
                await api.SaveAsync(request, ct);
                return Results.NoContent();
            }
            catch (AboutPageAdminException exception)
            {
                return ToResult(exception);
            }
        });

        return app;
    }

    private static IResult ToResult(AboutPageAdminException exception) => exception.Kind switch
    {
        _ => Results.BadRequest(exception.Message),
    };
}
