using System.ComponentModel.DataAnnotations;

namespace ProManagerOnline.Site.Web.Components.Pages;

/// <summary>The bound model for the public contact form, with the form's validation rules.</summary>
public sealed class ContactFormModel
{
    /// <summary>The visitor's name.</summary>
    [Required(ErrorMessage = "Please enter your name.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>The visitor's email address, used as the reply-to.</summary>
    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>The visitor's company, if given.</summary>
    [StringLength(120)]
    public string? Company { get; set; }

    /// <summary>The product the visitor is enquiring about, if any (carried from the pricing page).</summary>
    [StringLength(120)]
    public string? ProductOfInterest { get; set; }

    /// <summary>The visitor's message.</summary>
    [Required(ErrorMessage = "Please enter a message.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Please add a little more detail (at least 10 characters).")]
    public string Message { get; set; } = string.Empty;
}
