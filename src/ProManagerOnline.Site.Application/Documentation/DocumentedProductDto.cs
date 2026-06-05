namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>A read model summarising a documented product for the documentation landing page.</summary>
/// <param name="ProductId">The product's identifier, used to link to its documentation.</param>
/// <param name="ProductName">The product's display name.</param>
/// <param name="Summary">The product's one-line summary.</param>
/// <param name="ArticleCount">How many published articles the product has.</param>
public sealed record DocumentedProductDto(Guid ProductId, string ProductName, string Summary, int ArticleCount);
