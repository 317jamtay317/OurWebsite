namespace ProManagerOnline.Site.Infrastructure.Security;

/// <summary>
/// Configuration for Google reCAPTCHA, bound from the <c>Recaptcha</c> configuration section.
/// When <see cref="SecretKey"/> is blank the validator treats verification as disabled and lets
/// submissions through, so the site runs in development before keys are configured.
/// </summary>
public sealed class RecaptchaOptions
{
    /// <summary>The name of the configuration section these options bind to.</summary>
    public const string SectionName = "Recaptcha";

    /// <summary>The public site key, rendered into the page's reCAPTCHA widget.</summary>
    public string SiteKey { get; set; } = string.Empty;

    /// <summary>The secret key, used server-side to verify challenge responses. Never sent to the browser.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>The reCAPTCHA verification endpoint.</summary>
    public string VerifyUrl { get; set; } = "https://www.google.com/recaptcha/api/siteverify";
}
