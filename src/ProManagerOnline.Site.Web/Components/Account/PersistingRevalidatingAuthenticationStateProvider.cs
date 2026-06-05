using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Web.Client;

namespace ProManagerOnline.Site.Web.Components.Account;

/// <summary>
/// Supplies the authentication state to interactive Blazor components from the Identity cookie.
/// On the server circuit it revalidates the user's security stamp on an interval (so a password
/// change or sign-out invalidates an open circuit), and it also persists the signed-in user into
/// the page so that the Interactive Auto documentation admin can rebuild the same state once it is
/// running in WebAssembly.
/// </summary>
internal sealed class PersistingRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PersistentComponentState _state;
    private readonly IdentityOptions _options;
    private readonly PersistingComponentStateSubscription _subscription;
    private Task<AuthenticationState>? _authenticationStateTask;

    /// <summary>Initialises the provider and subscribes to persist the user for WebAssembly.</summary>
    /// <param name="loggerFactory">Logger factory used by the base revalidating provider.</param>
    /// <param name="scopeFactory">Creates a scope to resolve the scoped <see cref="UserManager{TUser}"/>.</param>
    /// <param name="persistentComponentState">Per-render state used to flow user info to WebAssembly.</param>
    /// <param name="optionsAccessor">Identity options, used to find the relevant claim types.</param>
    public PersistingRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        PersistentComponentState persistentComponentState,
        IOptions<IdentityOptions> optionsAccessor)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _state = persistentComponentState;
        _options = optionsAccessor.Value;

        AuthenticationStateChanged += OnAuthenticationStateChanged;
        _subscription = _state.RegisterOnPersisting(
            OnPersistingAsync, Microsoft.AspNetCore.Components.Web.RenderMode.InteractiveWebAssembly);
    }

    /// <inheritdoc />
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await ValidateSecurityStampAsync(userManager, authenticationState.User);
    }

    private async Task<bool> ValidateSecurityStampAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return false;
        }

        if (!userManager.SupportsUserSecurityStamp)
        {
            return true;
        }

        var principalStamp = principal.FindFirstValue(_options.ClaimsIdentity.SecurityStampClaimType);
        var userStamp = await userManager.GetSecurityStampAsync(user);
        return principalStamp == userStamp;
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> authenticationStateTask)
        => _authenticationStateTask = authenticationStateTask;

    private async Task OnPersistingAsync()
    {
        if (_authenticationStateTask is null)
        {
            throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
        }

        var principal = (await _authenticationStateTask).User;
        if (principal.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userId = principal.FindFirstValue(_options.ClaimsIdentity.UserIdClaimType);
        var email = principal.FindFirstValue(_options.ClaimsIdentity.EmailClaimType);
        if (userId is not null && email is not null)
        {
            _state.PersistAsJson(nameof(UserInfo), new UserInfo
            {
                UserId = userId,
                Email = email,
                Name = principal.Identity?.Name ?? email,
            });
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
        base.Dispose(disposing);
    }
}
