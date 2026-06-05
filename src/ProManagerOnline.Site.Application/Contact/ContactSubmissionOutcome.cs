namespace ProManagerOnline.Site.Application.Contact;

/// <summary>The result of submitting the public contact form.</summary>
public enum ContactSubmissionOutcome
{
    /// <summary>The enquiry passed the spam check and was emailed to the business inbox.</summary>
    Delivered,

    /// <summary>The enquiry failed the CAPTCHA check and was discarded as suspected spam.</summary>
    RejectedAsSpam,
}
