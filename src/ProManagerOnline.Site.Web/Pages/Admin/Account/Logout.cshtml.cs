using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProManagerOnline.Site.Infrastructure.Identity;

namespace ProManagerOnline.Site.Web.Pages.Admin.Account;

/// <summary>Signs the current admin out. Sign-out is POST-only to require an anti-forgery token.</summary>
public class LogoutModel(SignInManager<ApplicationUser> signInManager) : PageModel
{
    /// <summary>Redirects GET requests to the dashboard; there is no sign-out form to show.</summary>
    /// <returns>A redirect to the dashboard.</returns>
    public IActionResult OnGet() => RedirectToPage("/Admin/Index");

    /// <summary>Clears the auth cookie and returns to the sign-in page.</summary>
    /// <returns>A redirect to the sign-in page.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        await signInManager.SignOutAsync();
        return RedirectToPage("/Admin/Account/Login");
    }
}
