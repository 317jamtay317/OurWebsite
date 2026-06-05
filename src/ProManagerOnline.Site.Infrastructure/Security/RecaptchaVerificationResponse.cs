using System.Text.Json.Serialization;

namespace ProManagerOnline.Site.Infrastructure.Security;

/// <summary>The subset of Google's reCAPTCHA <c>siteverify</c> response the validator needs.</summary>
/// <param name="Success">Whether the challenge response was valid.</param>
internal sealed record RecaptchaVerificationResponse(
    [property: JsonPropertyName("success")] bool Success);
