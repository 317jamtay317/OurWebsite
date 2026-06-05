using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProManagerOnline.Site.Infrastructure.Security;
using ProManagerOnline.Site.Infrastructure.Tests.Support;
using Xunit;

namespace ProManagerOnline.Site.Infrastructure.Tests;

/// <summary>
/// Behaviour of the <see cref="GoogleRecaptchaValidator"/>: it verifies challenge responses with
/// Google when configured, fails closed when a token is missing, and bypasses when no secret is set.
/// </summary>
public class GoogleRecaptchaValidatorTests
{
    [Fact]
    public async Task IsHumanAsync_WhenGoogleReportsSuccess_ReturnsTrue()
    {
        var handler = new StubHttpMessageHandler("""{"success":true}""");
        var validator = CreateValidator(handler, secretKey: "secret");

        var result = await validator.IsHumanAsync("token", remoteIp: null, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task IsHumanAsync_WhenGoogleReportsFailure_ReturnsFalse()
    {
        var handler = new StubHttpMessageHandler("""{"success":false}""");
        var validator = CreateValidator(handler, secretKey: "secret");

        var result = await validator.IsHumanAsync("token", remoteIp: null, CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task IsHumanAsync_WhenResponseTokenMissing_ReturnsFalseWithoutCallingGoogle()
    {
        var handler = new StubHttpMessageHandler("""{"success":true}""");
        var validator = CreateValidator(handler, secretKey: "secret");

        var result = await validator.IsHumanAsync(response: "", remoteIp: null, CancellationToken.None);

        Assert.False(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task IsHumanAsync_WhenSecretKeyNotConfigured_AllowsWithoutCallingGoogle()
    {
        var handler = new StubHttpMessageHandler("""{"success":false}""");
        var validator = CreateValidator(handler, secretKey: "");

        var result = await validator.IsHumanAsync("token", remoteIp: null, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(0, handler.CallCount);
    }

    private static GoogleRecaptchaValidator CreateValidator(StubHttpMessageHandler handler, string secretKey)
    {
        var options = Options.Create(new RecaptchaOptions { SecretKey = secretKey });
        return new GoogleRecaptchaValidator(
            new HttpClient(handler), options, NullLogger<GoogleRecaptchaValidator>.Instance);
    }
}
