using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Web.Api;

namespace ProManagerOnline.Site.Web.Tests.Api;

/// <summary>
/// Verifies the documentation admin JSON API is mapped at the expected <c>/api/docs</c> routes.
/// These paths are the contract the WebAssembly client (<c>HttpClientDocsAdminApi</c>) calls, so a
/// rename on one side without the other would silently break the admin — this guards against that.
/// </summary>
public class DocsApiRoutingTests
{
    [Theory]
    [InlineData("GET", "/api/docs/products")]
    [InlineData("GET", "/api/docs/products/{productId:guid}/articles")]
    [InlineData("GET", "/api/docs/articles/{id:guid}")]
    [InlineData("POST", "/api/docs/articles")]
    [InlineData("PUT", "/api/docs/articles/{id:guid}")]
    [InlineData("POST", "/api/docs/articles/{id:guid}/publish")]
    [InlineData("POST", "/api/docs/articles/{id:guid}/unpublish")]
    [InlineData("POST", "/api/docs/media")]
    public void MapDocsAdminApi_MapsTheExpectedRoute(string method, string pattern)
    {
        var routes = MapAndCollectRoutes();

        Assert.Contains((method, pattern), routes);
    }

    private static HashSet<(string Method, string? Pattern)> MapAndCollectRoutes()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        builder.Services.AddScoped<IDocsAdminApi, StubDocsAdminApi>();

        var app = builder.Build();
        app.MapDocsAdminApi();

        // Endpoints are registered on the route builder's data sources; the DI EndpointDataSource is
        // only populated once the request pipeline is built, which a route-mapping test never does.
        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => (
                endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "",
                endpoint.RoutePattern.RawText))
            .ToHashSet();
    }
}
