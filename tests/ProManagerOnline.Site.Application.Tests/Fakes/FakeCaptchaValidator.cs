using ProManagerOnline.Site.Application.Security;

namespace ProManagerOnline.Site.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="ICaptchaValidator"/> test double. Returns a configurable verdict and
/// records the last challenge response and IP it was asked to validate, so handler tests can
/// assert what was checked.
/// </summary>
internal sealed class FakeCaptchaValidator : ICaptchaValidator
{
    public bool Result { get; set; } = true;

    public string? LastResponse { get; private set; }

    public string? LastRemoteIp { get; private set; }

    public Task<bool> IsHumanAsync(string? response, string? remoteIp, CancellationToken cancellationToken = default)
    {
        LastResponse = response;
        LastRemoteIp = remoteIp;
        return Task.FromResult(Result);
    }
}
