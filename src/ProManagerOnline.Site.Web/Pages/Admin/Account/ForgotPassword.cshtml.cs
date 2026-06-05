using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using ProManagerOnline.Site.Application.Administration;

namespace ProManagerOnline.Site.Web.Pages.Admin.Account;

/// <summary>Starts the forgot-password flow by emailing a reset link.</summary>
public class ForgotPasswordModel(RequestPasswordResetHandler requestReset) : PageModel
{
    /// <summary>The submitted email address.</summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>Whether the request has been processed (controls the neutral confirmation view).</summary>
    public bool RequestHandled { get; set; }

    /// <summary>The email entered on the form.</summary>
    public sealed class InputModel
    {
        /// <summary>The admin's email address.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>Shows the forgot-password form.</summary>
    public void OnGet()
    {
    }

    /// <summary>Requests a reset email, then always shows a neutral confirmation.</summary>
    /// <returns>The page, showing the confirmation message.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await requestReset.Handle(
            new RequestPasswordResetCommand(Input.Email),
            token =>
            {
                // The token can contain URL-unsafe characters; encode it for the query string.
                var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                return Url.Page(
                    "/Admin/Account/ResetPassword",
                    pageHandler: null,
                    values: new { email = Input.Email, token = encoded },
                    protocol: Request.Scheme)!;
            },
            HttpContext.RequestAborted);

        RequestHandled = true;
        return Page();
    }
}
