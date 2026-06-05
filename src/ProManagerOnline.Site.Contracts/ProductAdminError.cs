namespace ProManagerOnline.Site.Contracts;

/// <summary>The category of a failed product-admin operation, mapped to HTTP status codes.</summary>
public enum ProductAdminError
{
    /// <summary>The request was invalid (for example a blank name). Maps to HTTP 400.</summary>
    Invalid,

    /// <summary>The target product or plan does not exist. Maps to HTTP 404.</summary>
    NotFound,

    /// <summary>The request conflicts with current state (for example a duplicate slug). Maps to HTTP 409.</summary>
    Conflict,
}
