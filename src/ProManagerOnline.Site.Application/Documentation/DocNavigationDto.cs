namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>
/// A read model for a product's documentation navigation (the sidebar tree): the product's
/// name and its sections, each holding the articles within it.
/// </summary>
/// <param name="ProductId">The documented product's identifier.</param>
/// <param name="ProductName">The documented product's display name.</param>
/// <param name="Sections">The product's sections, ordered for display.</param>
public sealed record DocNavigationDto(Guid ProductId, string ProductName, IReadOnlyList<DocSectionDto> Sections);
