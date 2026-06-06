namespace ProManagerOnline.Site.Domain.Content;

/// <summary>
/// Persistence abstraction for the singleton <see cref="AboutPage"/> aggregate. Defined in the
/// domain and implemented in the infrastructure layer; consumed by application use cases.
/// <para>
/// It follows a unit-of-work model: <see cref="AddAsync"/> stages the page for insertion, mutations
/// to a page loaded by <see cref="GetAsync"/> are tracked, and <see cref="SaveChangesAsync"/>
/// commits either to the underlying store.
/// </para>
/// </summary>
public interface IAboutPageRepository
{
    /// <summary>Loads the single About page.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The About page, or <see langword="null"/> when it has not been created yet.</returns>
    Task<AboutPage?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Stages the About page for insertion; commit it with <see cref="SaveChangesAsync"/>.</summary>
    /// <param name="page">The page to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(AboutPage page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists every pending change — a newly added page or mutations made to a page loaded by
    /// <see cref="GetAsync"/> — to the underlying store.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
