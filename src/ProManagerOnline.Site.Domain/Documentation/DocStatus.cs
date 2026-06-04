namespace ProManagerOnline.Site.Domain.Documentation;

/// <summary>The publication state of a <see cref="DocArticle"/>.</summary>
public enum DocStatus
{
    /// <summary>Visible only in the admin; not shown on the public site.</summary>
    Draft,

    /// <summary>Live and visible on the public site.</summary>
    Published,
}
