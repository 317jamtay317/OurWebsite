using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProManagerOnline.Site.Web.Pages.Admin;

/// <summary>The admin dashboard landing page. Requires a signed-in admin.</summary>
public class IndexModel : PageModel
{
    /// <summary>The signed-in admin's email address, used as the display name.</summary>
    public string? Email { get; private set; }

    /// <summary>Reads the signed-in admin's identity for display.</summary>
    public void OnGet() => Email = User.Identity?.Name;
}
