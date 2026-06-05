namespace ProManagerOnline.Site.Domain.Products;

/// <summary>
/// A single feature line listed against a subscription <see cref="Plan"/> (for example
/// "Unlimited projects"). Owned by its <see cref="Plan"/> and ordered by
/// <see cref="Position"/>; created only through the plan, never directly.
/// </summary>
public sealed class PlanFeature
{
    // Parameterless constructor required by EF Core for materialisation. The domain
    // always creates features through the owning Plan; EF populates the properties.
    private PlanFeature() => Text = null!;

    private PlanFeature(int position, string text)
    {
        Position = position;
        Text = text;
    }

    /// <summary>The zero-based order of this feature within its plan's feature list.</summary>
    public int Position { get; private set; }

    /// <summary>The feature's display text.</summary>
    public string Text { get; private set; }

    /// <summary>Creates a feature at the given position. Internal: callable only by the owning <see cref="Plan"/>.</summary>
    /// <param name="position">The zero-based order of the feature within its plan.</param>
    /// <param name="text">The feature's display text; already trimmed and non-blank.</param>
    /// <returns>A new <see cref="PlanFeature"/>.</returns>
    internal static PlanFeature Create(int position, string text) => new(position, text);
}
