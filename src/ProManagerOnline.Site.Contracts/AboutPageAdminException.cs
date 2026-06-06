namespace ProManagerOnline.Site.Contracts;

/// <summary>
/// Raised by <see cref="IAboutPageAdminApi"/> implementations when an operation fails. Carries a
/// <see cref="Kind"/> so callers (Blazor components) can react uniformly whether the API ran in
/// process on the server or over HTTP from WebAssembly.
/// </summary>
public sealed class AboutPageAdminException : Exception
{
    /// <summary>Initialises a new <see cref="AboutPageAdminException"/>.</summary>
    /// <param name="kind">The category of failure.</param>
    /// <param name="message">A human-readable description of the failure.</param>
    public AboutPageAdminException(AboutPageAdminError kind, string message)
        : base(message)
        => Kind = kind;

    /// <summary>The category of failure.</summary>
    public AboutPageAdminError Kind { get; }
}
