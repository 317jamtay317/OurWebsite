namespace ProManagerOnline.Site.Web.Client;

/// <summary>
/// The signed-in admin, persisted from the server into the page so the WebAssembly runtime can
/// rebuild the authentication state without a server round-trip. Shared by the server's persisting
/// provider and the client's <see cref="Services.PersistentAuthenticationStateProvider"/>.
/// </summary>
public sealed class UserInfo
{
    /// <summary>The user's identifier.</summary>
    public required string UserId { get; init; }

    /// <summary>The user's email address.</summary>
    public required string Email { get; init; }

    /// <summary>The user's display name (falls back to the email).</summary>
    public required string Name { get; init; }
}
