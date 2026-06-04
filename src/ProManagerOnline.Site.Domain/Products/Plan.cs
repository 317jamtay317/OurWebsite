using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Domain.Products;

/// <summary>
/// A subscription tier within a <see cref="Product"/> (for example "Solo" or "Team").
/// An entity of the <see cref="Product"/> aggregate: it is always created and modified
/// through its owning product, never directly.
/// </summary>
public sealed class Plan
{
    // Parameterless constructor required by EF Core for materialisation. The domain
    // always creates plans through Create(...); EF populates the properties afterwards.
    private Plan()
    {
        Name = null!;
        Description = null!;
        Price = null!;
    }

    private Plan(PlanId id, string name, string description, Money price, BillingPeriod billingPeriod)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        BillingPeriod = billingPeriod;
        IsFeatured = false;
    }

    /// <summary>The plan's unique identifier within its product.</summary>
    public PlanId Id { get; }

    /// <summary>The plan's display name, for example "Team".</summary>
    public string Name { get; private set; }

    /// <summary>A short description of who the plan suits.</summary>
    public string Description { get; private set; }

    /// <summary>The recurring price charged for the plan.</summary>
    public Money Price { get; private set; }

    /// <summary>How often <see cref="Price"/> is billed.</summary>
    public BillingPeriod BillingPeriod { get; private set; }

    /// <summary>Whether this plan is highlighted as the recommended tier.</summary>
    public bool IsFeatured { get; private set; }

    /// <summary>Creates a plan. Internal: callable only by the owning <see cref="Product"/>.</summary>
    internal static Plan Create(string name, string description, Money price, BillingPeriod billingPeriod)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A plan must have a name.");
        }

        return new Plan(PlanId.New(), name.Trim(), description.Trim(), price, billingPeriod);
    }

    /// <summary>Replaces the plan's price. Internal: callable only by the owning product.</summary>
    internal void ChangePrice(Money price) => Price = price;

    /// <summary>Marks the plan as featured. Internal: callable only by the owning product.</summary>
    internal void Feature() => IsFeatured = true;

    /// <summary>Clears the plan's featured flag. Internal: callable only by the owning product.</summary>
    internal void Unfeature() => IsFeatured = false;
}
