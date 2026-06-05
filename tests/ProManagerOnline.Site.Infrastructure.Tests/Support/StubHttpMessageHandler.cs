using System.Net;
using System.Text;

namespace ProManagerOnline.Site.Infrastructure.Tests.Support;

/// <summary>
/// A test <see cref="HttpMessageHandler"/> that returns a fixed JSON body for every request and
/// counts how many requests it received, so HTTP-backed services can be exercised without a network.
/// </summary>
/// <param name="jsonResponse">The JSON body to return for every request.</param>
internal sealed class StubHttpMessageHandler(string jsonResponse) : HttpMessageHandler
{
    /// <summary>The number of requests this handler has received.</summary>
    public int CallCount { get; private set; }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json"),
        };

        return Task.FromResult(response);
    }
}
