namespace ProManagerOnline.Site.Application.Security;

/// <summary>
/// Verifies that a form submission was made by a human, using a CAPTCHA challenge response.
/// Defined in the application layer and implemented in infrastructure against the CAPTCHA
/// provider, so use cases can gate on "is this a human" without knowing the provider.
/// </summary>
public interface ICaptchaValidator
{
    /// <summary>Determines whether a CAPTCHA challenge response represents a genuine human.</summary>
    /// <param name="response">The challenge response token from the client widget, if any.</param>
    /// <param name="remoteIp">The submitter's IP address, if known; helps the provider score the request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if the submission is judged human; otherwise <see langword="false"/>.</returns>
    Task<bool> IsHumanAsync(string? response, string? remoteIp, CancellationToken cancellationToken = default);
}
