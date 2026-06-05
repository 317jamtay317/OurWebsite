namespace ProManagerOnline.Site.Application.Exceptions;

/// <summary>
/// Thrown by an application use case when a request targets an entity that does not exist —
/// for example, editing a documentation article by an identifier that is not in the store.
/// Maps naturally to an HTTP 404 response in the web layer.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>Initialises a new <see cref="NotFoundException"/>.</summary>
    /// <param name="message">A human-readable description of what could not be found.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }
}
