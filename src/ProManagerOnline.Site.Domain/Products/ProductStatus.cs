namespace ProManagerOnline.Site.Domain.Products;

/// <summary>The publication state of a <see cref="Product"/>.</summary>
public enum ProductStatus
{
    /// <summary>Visible only in the admin; not shown on the public site.</summary>
    Draft,

    /// <summary>Live and visible on the public site.</summary>
    Published,
}
