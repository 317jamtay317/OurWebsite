namespace ProManagerOnline.Site.Application.Administration;

/// <summary>
/// The outcome of an admin account operation (change password, reset password).
/// Carries a success flag and any human-readable error messages, so the
/// application layer never has to surface infrastructure-specific identity types.
/// </summary>
public sealed class AdminAccountResult
{
    private AdminAccountResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    /// <summary>Whether the operation completed successfully.</summary>
    public bool Succeeded { get; }

    /// <summary>The error messages explaining a failure; empty when <see cref="Succeeded"/> is <see langword="true"/>.</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>Creates a successful result with no errors.</summary>
    /// <returns>A succeeded <see cref="AdminAccountResult"/>.</returns>
    public static AdminAccountResult Success() => new(true, []);

    /// <summary>Creates a failed result carrying the given error messages.</summary>
    /// <param name="errors">The messages explaining why the operation failed.</param>
    /// <returns>A failed <see cref="AdminAccountResult"/>.</returns>
    public static AdminAccountResult Failure(params string[] errors) => new(false, errors);

    /// <summary>Creates a failed result carrying the given error messages.</summary>
    /// <param name="errors">The messages explaining why the operation failed.</param>
    /// <returns>A failed <see cref="AdminAccountResult"/>.</returns>
    public static AdminAccountResult Failure(IEnumerable<string> errors) => new(false, errors.ToList());
}
