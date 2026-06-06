namespace ProManagerOnline.Site.Contracts;

/// <summary>The category of a failed About-page admin operation, mapped to HTTP status codes.</summary>
public enum AboutPageAdminError
{
    /// <summary>The request was invalid (for example a blank title or body). Maps to HTTP 400.</summary>
    Invalid,
}
