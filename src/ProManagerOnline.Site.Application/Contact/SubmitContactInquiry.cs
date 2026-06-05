using System.Net;
using ProManagerOnline.Site.Application.Email;
using ProManagerOnline.Site.Application.Security;

namespace ProManagerOnline.Site.Application.Contact;

/// <summary>The details a visitor submits through the public contact form.</summary>
/// <param name="Name">The visitor's name.</param>
/// <param name="Email">The visitor's email address, used as the reply-to.</param>
/// <param name="Company">The visitor's company, if given.</param>
/// <param name="ProductOfInterest">The product the visitor is enquiring about, if any (carried from the pricing page).</param>
/// <param name="Message">The visitor's message.</param>
/// <param name="CaptchaResponse">The reCAPTCHA challenge response token from the widget.</param>
/// <param name="RemoteIp">The visitor's IP address, if known.</param>
public sealed record SubmitContactInquiryCommand(
    string Name,
    string Email,
    string? Company,
    string? ProductOfInterest,
    string Message,
    string? CaptchaResponse,
    string? RemoteIp);

/// <summary>
/// Handles a public contact-form submission: it first verifies the submission is human via the
/// CAPTCHA, and only then emails the enquiry to the business inbox. Submissions that fail the
/// CAPTCHA are discarded, so the inbox is protected from automated spam.
/// </summary>
/// <param name="captcha">The CAPTCHA validator that gates out bots.</param>
/// <param name="emailSender">The email sender used to deliver the enquiry.</param>
public sealed class SubmitContactInquiryHandler(ICaptchaValidator captcha, IEmailSender emailSender)
{
    /// <summary>Verifies the submission and, when human, emails it to the business inbox.</summary>
    /// <param name="command">The submitted enquiry.</param>
    /// <param name="recipient">The business inbox the enquiry is emailed to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <see cref="ContactSubmissionOutcome.Delivered"/> when the enquiry was emailed, or
    /// <see cref="ContactSubmissionOutcome.RejectedAsSpam"/> when it failed the CAPTCHA.
    /// </returns>
    public async Task<ContactSubmissionOutcome> Handle(
        SubmitContactInquiryCommand command, string recipient, CancellationToken cancellationToken = default)
    {
        var isHuman = await captcha.IsHumanAsync(command.CaptchaResponse, command.RemoteIp, cancellationToken);
        if (!isHuman)
        {
            return ContactSubmissionOutcome.RejectedAsSpam;
        }

        await emailSender.SendAsync(recipient, BuildSubject(command), BuildBody(command), cancellationToken);
        return ContactSubmissionOutcome.Delivered;
    }

    private static string BuildSubject(SubmitContactInquiryCommand command) =>
        string.IsNullOrWhiteSpace(command.ProductOfInterest)
            ? $"New website enquiry from {command.Name}"
            : $"New website enquiry from {command.Name} about {command.ProductOfInterest}";

    private static string BuildBody(SubmitContactInquiryCommand command)
    {
        var company = string.IsNullOrWhiteSpace(command.Company) ? "—" : command.Company;
        var product = string.IsNullOrWhiteSpace(command.ProductOfInterest) ? "—" : command.ProductOfInterest;

        return
            "<p>A new enquiry was submitted through the ProManager Online contact form.</p>" +
            "<ul>" +
            $"<li><strong>Name:</strong> {WebUtility.HtmlEncode(command.Name)}</li>" +
            $"<li><strong>Email:</strong> {WebUtility.HtmlEncode(command.Email)}</li>" +
            $"<li><strong>Company:</strong> {WebUtility.HtmlEncode(company)}</li>" +
            $"<li><strong>Product of interest:</strong> {WebUtility.HtmlEncode(product)}</li>" +
            "</ul>" +
            "<p><strong>Message:</strong></p>" +
            $"<p>{WebUtility.HtmlEncode(command.Message)}</p>";
    }
}
