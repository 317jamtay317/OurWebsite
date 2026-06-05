using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using ProManagerOnline.Site.Application.Administration;

namespace ProManagerOnline.Site.Web.Pages.Admin.Account;

/// <summary>Completes the forgot-password flow by setting a new password from a reset link.</summary>
public class ResetPasswordModel(ResetAdminPasswordHandler resetPassword) : PageModel
{
    /// <summary>The submitted reset details.</summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>Whether the password was successfully reset (controls the confirmation view).</summary>
    public bool Succeeded { get; set; }

    /// <summary>The email, token and new password backing the reset form.</summary>
    public sealed class InputModel
    {
        /// <summary>The admin's email address, carried from the reset link.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>The encoded reset token, carried from the reset link.</summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        /// <summary>The new password to set.</summary>
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>Confirmation of the new password.</summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>Shows the reset form, seeded from the emailed link's email and token.</summary>
    /// <param name="email">The admin's email address.</param>
    /// <param name="token">The encoded reset token.</param>
    /// <returns>The form, or a bad request when the link is incomplete.</returns>
    public IActionResult OnGet(string? email, string? token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return BadRequest("A valid password reset link is required.");
        }

        Input.Email = email;
        Input.Token = token;
        return Page();
    }

    /// <summary>Decodes the token and applies the new password.</summary>
    /// <returns>The confirmation view on success, otherwise the form with errors.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Token));
        }
        catch (FormatException)
        {
            ModelState.AddModelError(string.Empty, "Invalid or expired reset link.");
            return Page();
        }

        var result = await resetPassword.Handle(
            new ResetAdminPasswordCommand(Input.Email, token, Input.Password),
            HttpContext.RequestAborted);

        if (result.Succeeded)
        {
            Succeeded = true;
            return Page();
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return Page();
    }
}
