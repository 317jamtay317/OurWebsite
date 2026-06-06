namespace ProManagerOnline.Site.Contracts;

/// <summary>A published product that documentation can be attached to, as a picker option.</summary>
/// <param name="Id">The product's identifier.</param>
/// <param name="Name">The product's display name.</param>
public sealed record ProductOption(Guid Id, string Name);
