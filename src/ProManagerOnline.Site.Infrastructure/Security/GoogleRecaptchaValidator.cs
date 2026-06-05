using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProManagerOnline.Site.Application.Security;

namespace ProManagerOnline.Site.Infrastructure.Security;

/// <summary>
/// <see cref="ICaptchaValidator"/> backed by Google reCAPTCHA v2. Verifies a challenge response by
/// calling the reCAPTCHA <c>siteverify</c> endpoint with the configured secret key. When no secret
/// key is configured, verification is treated as disabled and every submission is allowed, so the
/// site works in development before keys are set up. Network or parsing failures fail closed (reject).
/// </summary>
/// <param name="httpClient">The HTTP client used to call the reCAPTCHA endpoint.</param>
/// <param name="options">The reCAPTCHA configuration.</param>
/// <param name="logger">The logger.</param>
public sealed class GoogleRecaptchaValidator(
    HttpClient httpClient,
    IOptions<RecaptchaOptions> options,
    ILogger<GoogleRecaptchaValidator> logger) : ICaptchaValidator
{
    /// <inheritdoc />
    public async Task<bool> IsHumanAsync(
        string? response, string? remoteIp, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.SecretKey))
        {
            logger.LogWarning(
                "reCAPTCHA secret key is not configured; allowing the submission without verification.");
            return true;
        }

        if (string.IsNullOrWhiteSpace(response))
        {
            return false;
        }

        try
        {
            using var request = BuildVerificationRequest(settings.SecretKey, response, remoteIp);
            using var httpResponse = await httpClient.PostAsync(settings.VerifyUrl, request, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();

            var json = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var verification = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(json, SerializerOptions);

            return verification?.Success ?? false;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogError(exception, "reCAPTCHA verification could not be completed; rejecting the submission.");
            return false;
        }
    }

    private static FormUrlEncodedContent BuildVerificationRequest(string secret, string response, string? remoteIp)
    {
        var fields = new Dictionary<string, string>
        {
            ["secret"] = secret,
            ["response"] = response,
        };

        if (!string.IsNullOrWhiteSpace(remoteIp))
        {
            fields["remoteip"] = remoteIp;
        }

        return new FormUrlEncodedContent(fields);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
}
