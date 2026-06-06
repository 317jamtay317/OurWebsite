namespace ProManagerOnline.Site.Contracts;

/// <summary>
/// Raised by <see cref="IDocsAdminApi"/> implementations when an operation fails. Carries a
/// <see cref="Kind"/> so callers (Blazor components) can react uniformly whether the API ran in
/// process on the server or over HTTP from WebAssembly.
/// </summary>
public sealed class DocsAdminException : Exception
{
    /// <summary>Initialises a new <see cref="DocsAdminException"/>.</summary>
    /// <param name="kind">The category of failure.</param>
    /// <param name="message">A human-readable description of the failure.</param>
    public DocsAdminException(DocsAdminError kind, string message)
        : base(message)
        => Kind = kind;

    /// <summary>The category of failure.</summary>
    public DocsAdminError Kind { get; }
}
