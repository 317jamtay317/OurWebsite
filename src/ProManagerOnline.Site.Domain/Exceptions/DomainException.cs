namespace ProManagerOnline.Site.Domain.Exceptions;

/// <summary>
/// Base type for every exception that represents the violation of a domain rule
/// (an invariant of the model). Catch this to distinguish business-rule failures
/// from technical or infrastructure errors.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Initialises a new <see cref="DomainException"/> describing the violated rule.
    /// </summary>
    /// <param name="message">A human-readable description of the rule that was broken.</param>
    public DomainException(string message)
        : base(message)
    {
    }
}
