using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ProManagerOnline.Site.Web.Client.Services;

/// <summary>
/// Rebuilds the authentication state in WebAssembly from the <see cref="UserInfo"/> the server
/// persisted into the page. The state is fixed for the lifetime of the WebAssembly session; the
/// server revalidates the security stamp and a full reload picks up any change.
/// </summary>
internal sealed class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> Unauthenticated =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private readonly Task<AuthenticationState> _authenticationStateTask = Unauthenticated;

    /// <summary>Reads the persisted user, if any, and builds the authentication state from it.</summary>
    /// <param name="state">The persisted component state flowed from the server.</param>
    public PersistentAuthenticationStateProvider(PersistentComponentState state)
    {
        if (!state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) || userInfo is null)
        {
            return;
        }

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
            new Claim(ClaimTypes.Name, userInfo.Name),
            new Claim(ClaimTypes.Email, userInfo.Email),
        ];

        _authenticationStateTask = Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(
                new ClaimsIdentity(claims, authenticationType: nameof(PersistentAuthenticationStateProvider)))));
    }

    /// <inheritdoc />
    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authenticationStateTask;
}
