using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProManagerOnline.Site.Infrastructure.Identity;

namespace ProManagerOnline.Site.Web.Pages.Admin.Account;

/// <summary>The admin sign-in page. Issues the auth cookie via the Identity sign-in manager.</summary>
public class LoginModel(SignInManager<ApplicationUser> signInManager) : PageModel
{
    /// <summary>The submitted credentials.</summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>Where to send the admin after a successful sign-in.</summary>
    public string? ReturnUrl { get; set; }

    /// <summary>Credentials entered on the sign-in form.</summary>
    public sealed class InputModel
    {
        /// <summary>The admin's email address.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>The admin's password.</summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>Whether to keep the admin signed in across browser sessions.</summary>
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    /// <summary>Shows the sign-in form.</summary>
    /// <param name="returnUrl">The page to return to after signing in.</param>
    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    /// <summary>Validates the credentials and signs the admin in.</summary>
    /// <param name="returnUrl">The page to return to after signing in.</param>
    /// <returns>A redirect on success, otherwise the form re-rendered with an error.</returns>
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl = string.IsNullOrEmpty(returnUrl) ? Url.Content("~/Admin") : returnUrl;
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
    }
}
