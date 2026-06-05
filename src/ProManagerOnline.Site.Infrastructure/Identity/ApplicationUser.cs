using Microsoft.AspNetCore.Identity;

namespace ProManagerOnline.Site.Infrastructure.Identity;

/// <summary>
/// An admin account for the site CMS. Backed by ASP.NET Core Identity, so password
/// hashing, lockout and reset tokens are handled by the framework. Admin identity is an
/// infrastructure/security concern and deliberately lives outside the (pure) domain model.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    /// <summary>The admin's display name, shown in the admin UI. Optional.</summary>
    public string? DisplayName { get; set; }
}
