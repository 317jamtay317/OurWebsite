using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProManagerOnline.Site.Application.Administration;
using ProManagerOnline.Site.Infrastructure.Identity;

namespace ProManagerOnline.Site.Web.Pages.Admin;

/// <summary>
/// Lets the signed-in admin change their own password. Lives under <c>/Admin</c> (not
/// <c>/Admin/Account</c>) so it stays behind the sign-in requirement.
/// </summary>
public class ChangePasswordModel(
    ChangeAdminPasswordHandler changePassword,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : PageModel
{
    /// <summary>The submitted passwords.</summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>A confirmation message shown after a successful change.</summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>The current and new passwords entered on the form.</summary>
    public sealed class InputModel
    {
        /// <summary>The admin's existing password.</summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>The new password to set.</summary>
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>Confirmation of the new password.</summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>Shows the change-password form.</summary>
    public void OnGet()
    {
    }

    /// <summary>Validates and applies the password change.</summary>
    /// <returns>The form with errors, or a redirect showing a success message.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var adminId = userManager.GetUserId(User)!;
        var result = await changePassword.Handle(
            new ChangeAdminPasswordCommand(adminId, Input.CurrentPassword, Input.NewPassword),
            HttpContext.RequestAborted);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return Page();
        }

        // Changing the password rotates the security stamp; refresh the cookie so the admin stays signed in.
        var user = await userManager.GetUserAsync(User);
        if (user is not null)
        {
            await signInManager.RefreshSignInAsync(user);
        }

        StatusMessage = "Your password has been changed.";
        return RedirectToPage();
    }
}
