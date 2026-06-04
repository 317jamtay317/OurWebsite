using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Domain.Products;

/// <summary>
/// A product in the catalogue (for example Workflows.AI). The aggregate root for its
/// pricing <see cref="Plan"/>s. A product is created as a <see cref="ProductStatus.Draft"/>,
/// priced either with tiered plans or as quote-based, and then published.
/// </summary>
public sealed class Product
{
    private readonly List<Plan> _plans = [];

    private Product(ProductId id, Slug slug, string name, string category, string summary)
    {
        Id = id;
        Slug = slug;
        Name = name;
        Category = category;
        Summary = summary;
        Status = ProductStatus.Draft;
        PricingKind = PricingKind.Tiered;
    }

    /// <summary>The product's unique identifier.</summary>
    public ProductId Id { get; }

    /// <summary>The URL-safe slug used in the product's public links.</summary>
    public Slug Slug { get; private set; }

    /// <summary>The product's display name, for example "Workflows.AI".</summary>
    public string Name { get; private set; }

    /// <summary>The product's category label, for example "Business management".</summary>
    public string Category { get; private set; }

    /// <summary>A one-line summary shown on listings and the product hero.</summary>
    public string Summary { get; private set; }

    /// <summary>Whether the product is a draft or published.</summary>
    public ProductStatus Status { get; private set; }

    /// <summary>How the product is priced: subscription tiers, a one-time fixed price, or quote-based.</summary>
    public PricingKind PricingKind { get; private set; }

    /// <summary>
    /// The one-time price when <see cref="PricingKind"/> is <see cref="PricingKind.Fixed"/>;
    /// otherwise <see langword="null"/>.
    /// </summary>
    public Money? FixedPrice { get; private set; }

    /// <summary>The product's subscription plans. Empty unless the product uses tiered pricing.</summary>
    public IReadOnlyCollection<Plan> Plans => _plans.AsReadOnly();

    /// <summary>
    /// Creates a new draft product, priced as tiered with no plans yet.
    /// </summary>
    /// <param name="slug">The URL-safe slug for the product's links.</param>
    /// <param name="name">The display name; must not be blank.</param>
    /// <param name="category">The category label; must not be blank.</param>
    /// <param name="summary">A one-line summary; must not be blank.</param>
    /// <returns>A new <see cref="Product"/> in the <see cref="ProductStatus.Draft"/> state.</returns>
    /// <exception cref="DomainException">Thrown when <paramref name="name"/>, <paramref name="category"/> or <paramref name="summary"/> is blank.</exception>
    public static Product CreateDraft(Slug slug, string name, string category, string summary)
    {
        Require(name, nameof(name));
        Require(category, nameof(category));
        Require(summary, nameof(summary));

        return new Product(ProductId.New(), slug, name.Trim(), category.Trim(), summary.Trim());
    }

    /// <summary>
    /// Updates the product's display details.
    /// </summary>
    /// <param name="name">The new display name; must not be blank.</param>
    /// <param name="category">The new category label; must not be blank.</param>
    /// <param name="summary">The new one-line summary; must not be blank.</param>
    /// <exception cref="DomainException">Thrown when <paramref name="name"/>, <paramref name="category"/> or <paramref name="summary"/> is blank.</exception>
    public void UpdateDetails(string name, string category, string summary)
    {
        Require(name, nameof(name));
        Require(category, nameof(category));
        Require(summary, nameof(summary));

        Name = name.Trim();
        Category = category.Trim();
        Summary = summary.Trim();
    }

    /// <summary>
    /// Changes the product's URL-safe slug. Only allowed while the product is a draft, because a
    /// published product's slug is part of its live public links.
    /// </summary>
    /// <param name="slug">The new slug.</param>
    /// <exception cref="DomainException">Thrown when the product is published.</exception>
    public void ChangeSlug(Slug slug)
    {
        if (Status == ProductStatus.Published)
        {
            throw new DomainException("A published product's slug cannot be changed; unpublish it first.");
        }

        Slug = slug;
    }

    /// <summary>
    /// Adds a subscription plan (tier) to a tiered product.
    /// </summary>
    /// <param name="name">The plan name, for example "Team".</param>
    /// <param name="description">A short description of who the plan suits.</param>
    /// <param name="price">The recurring price.</param>
    /// <param name="billingPeriod">How often the price is billed.</param>
    /// <param name="features">The feature lines to list against the plan; blank entries are ignored.</param>
    /// <returns>The identifier of the newly added plan.</returns>
    /// <exception cref="DomainException">Thrown when the product is not tiered, or the plan name is blank.</exception>
    public PlanId AddPlan(
        string name, string description, Money price, BillingPeriod billingPeriod, IEnumerable<string>? features = null)
    {
        RequireTiered();

        var plan = Plan.Create(name, description, price, billingPeriod, features ?? []);
        _plans.Add(plan);
        return plan.Id;
    }

    /// <summary>
    /// Updates one of the product's plans, replacing its details and features.
    /// </summary>
    /// <param name="planId">The plan to update.</param>
    /// <param name="name">The new plan name; must not be blank.</param>
    /// <param name="description">The new description of who the plan suits.</param>
    /// <param name="price">The new recurring price.</param>
    /// <param name="billingPeriod">How often the price is billed.</param>
    /// <param name="features">The feature lines to list against the plan; blank entries are ignored.</param>
    /// <exception cref="DomainException">Thrown when the product has no plan with the given id, or the plan name is blank.</exception>
    public void UpdatePlan(
        PlanId planId, string name, string description, Money price, BillingPeriod billingPeriod, IEnumerable<string> features)
        => FindPlan(planId).Update(name, description, price, billingPeriod, features);

    /// <summary>
    /// Removes one of the product's plans.
    /// </summary>
    /// <param name="planId">The plan to remove.</param>
    /// <exception cref="DomainException">Thrown when the product has no plan with the given id.</exception>
    public void RemovePlan(PlanId planId) => _plans.Remove(FindPlan(planId));

    /// <summary>
    /// Changes the price of one of the product's plans.
    /// </summary>
    /// <param name="planId">The plan to reprice.</param>
    /// <param name="newPrice">The new recurring price.</param>
    /// <exception cref="DomainException">Thrown when the product has no plan with the given id.</exception>
    public void ChangePlanPrice(PlanId planId, Money newPrice) => FindPlan(planId).ChangePrice(newPrice);

    /// <summary>
    /// Marks a plan as the featured (recommended) tier, clearing the flag from every other plan.
    /// </summary>
    /// <param name="planId">The plan to feature.</param>
    /// <exception cref="DomainException">Thrown when the product has no plan with the given id.</exception>
    public void FeaturePlan(PlanId planId)
    {
        var plan = FindPlan(planId);

        foreach (var other in _plans)
        {
            other.Unfeature();
        }

        plan.Feature();
    }

    /// <summary>
    /// Switches the product to subscription (tiered) pricing, clearing any fixed price. Plans
    /// can then be added with <see cref="AddPlan"/>.
    /// </summary>
    public void MakeTiered()
    {
        PricingKind = PricingKind.Tiered;
        FixedPrice = null;
    }

    /// <summary>
    /// Switches the product to a single one-time fixed price, discarding any existing plans.
    /// </summary>
    /// <param name="price">The one-time price.</param>
    public void MakeFixedPrice(Money price)
    {
        PricingKind = PricingKind.Fixed;
        FixedPrice = price;
        _plans.Clear();
    }

    /// <summary>
    /// Changes the one-time price of a fixed-price product.
    /// </summary>
    /// <param name="price">The new one-time price.</param>
    /// <exception cref="DomainException">Thrown when the product is not fixed-price.</exception>
    public void ChangeFixedPrice(Money price)
    {
        if (PricingKind != PricingKind.Fixed)
        {
            throw new DomainException("Only a fixed-price product has a fixed price to change.");
        }

        FixedPrice = price;
    }

    /// <summary>
    /// Switches the product to quote-based pricing, discarding any existing plans and fixed price.
    /// </summary>
    public void MakeQuoteBased()
    {
        PricingKind = PricingKind.Quote;
        FixedPrice = null;
        _plans.Clear();
    }

    /// <summary>
    /// Publishes the product so it is visible on the public site.
    /// </summary>
    /// <exception cref="DomainException">Thrown when a tiered product has no plans.</exception>
    public void Publish()
    {
        if (PricingKind == PricingKind.Tiered && _plans.Count == 0)
        {
            throw new DomainException("A tiered product needs at least one plan before it can be published.");
        }

        Status = ProductStatus.Published;
    }

    /// <summary>
    /// Returns a published product to the draft state, removing it from the public site so it
    /// can be edited freely.
    /// </summary>
    public void Unpublish() => Status = ProductStatus.Draft;

    private Plan FindPlan(PlanId planId) =>
        _plans.FirstOrDefault(plan => plan.Id == planId)
        ?? throw new DomainException($"This product has no plan with id {planId.Value}.");

    private void RequireTiered()
    {
        if (PricingKind != PricingKind.Tiered)
        {
            throw new DomainException("Plans can only be managed on a subscription (tiered) product.");
        }
    }

    private static void Require(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"A product's {field} must not be blank.");
        }
    }
}
