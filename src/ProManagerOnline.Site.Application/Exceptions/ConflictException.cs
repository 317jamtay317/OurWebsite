namespace ProManagerOnline.Site.Application.Exceptions;

/// <summary>
/// Thrown by an application use case when a request conflicts with the current state of
/// the system — for example, creating a product with a slug that is already in use.
/// Maps naturally to an HTTP 409 response in the web layer.
/// </summary>
public sealed class ConflictException : Exception
{
    /// <summary>Initialises a new <see cref="ConflictException"/>.</summary>
    /// <param name="message">A human-readable description of the conflict.</param>
    public ConflictException(string message)
        : base(message)
    {
    }
}
