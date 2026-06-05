namespace ProManagerOnline.Site.Application.Exceptions;

/// <summary>
/// Thrown by an application use case when the entity a request targets does not exist —
/// for example, editing a product that has been deleted. Maps naturally to an HTTP 404
/// response in the web layer.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>Initialises a new <see cref="NotFoundException"/>.</summary>
    /// <param name="message">A human-readable description of what was not found.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }
}
