using ProManagerOnline.Site.Contracts;

namespace ProManagerOnline.Site.Web.Api;

/// <summary>JSON endpoints for the product admin, consumed by the WebAssembly client.</summary>
public static class ProductsApi
{
    /// <summary>Maps the <c>/api/products</c> endpoints over <see cref="IProductAdminApi"/>.</summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The same route builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapProductAdminApi(this IEndpointRouteBuilder app)
    {
        // The whole product admin API requires an authenticated admin (cookie auth).
        var group = app.MapGroup("/api/products").RequireAuthorization();

        group.MapGet("/", (IProductAdminApi api, CancellationToken ct) => Run(() => api.ListAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, IProductAdminApi api, CancellationToken ct) =>
        {
            try
            {
                var product = await api.GetAsync(id, ct);
                return product is null ? Results.NotFound() : Results.Ok(product);
            }
            catch (ProductAdminException exception)
            {
                return ToResult(exception);
            }
        });

        group.MapPost("/", (CreateProductRequest request, IProductAdminApi api, CancellationToken ct)
            => Run(() => api.CreateAsync(request, ct)));

        group.MapPut("/{id:guid}/details", (Guid id, UpdateProductDetailsRequest request, IProductAdminApi api, CancellationToken ct)
            => Run(() => api.UpdateDetailsAsync(id, request, ct)));

        group.MapPut("/{id:guid}/pricing", (Guid id, SetProductPricingRequest request, IProductAdminApi api, CancellationToken ct)
            => Run(() => api.SetPricingAsync(id, request, ct)));

        group.MapDelete("/{id:guid}", (Guid id, IProductAdminApi api, CancellationToken ct)
            => Run(() => api.DeleteAsync(id, ct)));

        group.MapPost("/{id:guid}/publish", (Guid id, IProductAdminApi api, CancellationToken ct)
            => Run(() => api.PublishAsync(id, ct)));

        group.MapPost("/{id:guid}/unpublish", (Guid id, IProductAdminApi api, CancellationToken ct)
            => Run(() => api.UnpublishAsync(id, ct)));

        group.MapPost("/{id:guid}/plans", (Guid id, AddPlanRequest request, IProductAdminApi api, CancellationToken ct)
            => Run(() => api.AddPlanAsync(id, request, ct)));

        group.MapPut("/{id:guid}/plans/{planId:guid}", (Guid id, Guid planId, UpdatePlanRequest request, IProductAdminApi api, CancellationToken ct)
            => Run(() => api.UpdatePlanAsync(id, planId, request, ct)));

        group.MapDelete("/{id:guid}/plans/{planId:guid}", (Guid id, Guid planId, IProductAdminApi api, CancellationToken ct)
            => Run(() => api.RemovePlanAsync(id, planId, ct)));

        group.MapPost("/{id:guid}/plans/{planId:guid}/feature", (Guid id, Guid planId, IProductAdminApi api, CancellationToken ct)
            => Run(() => api.FeaturePlanAsync(id, planId, ct)));

        return app;
    }

    private static async Task<IResult> Run(Func<Task> action)
    {
        try
        {
            await action();
            return Results.NoContent();
        }
        catch (ProductAdminException exception)
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
        catch (ProductAdminException exception)
        {
            return ToResult(exception);
        }
    }

    private static IResult ToResult(ProductAdminException exception) => exception.Kind switch
    {
        ProductAdminError.NotFound => Results.NotFound(exception.Message),
        ProductAdminError.Conflict => Results.Conflict(exception.Message),
        _ => Results.BadRequest(exception.Message),
    };
}
