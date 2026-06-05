namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>A read model grouping a product's documentation articles under a named section.</summary>
/// <param name="Section">The section's name, for example "Getting started".</param>
/// <param name="Articles">The section's articles, ordered by position.</param>
public sealed record DocSectionDto(string Section, IReadOnlyList<DocArticleSummaryDto> Articles);
