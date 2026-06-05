namespace ProManagerOnline.Site.Infrastructure.Email;

/// <summary>
/// SMTP configuration for delivering transactional email, bound from the <c>Smtp</c>
/// configuration section. When <see cref="Host"/> is blank the application falls back to the
/// logging email sender, so the site runs in development before a mail server is configured.
/// </summary>
public sealed class SmtpOptions
{
    /// <summary>The name of the configuration section these options bind to.</summary>
    public const string SectionName = "Smtp";

    /// <summary>The SMTP server host name.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>The SMTP server port. Defaults to the submission port, 587.</summary>
    public int Port { get; set; } = 587;

    /// <summary>Whether to connect using SSL/TLS.</summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>The user name used to authenticate with the SMTP server.</summary>
    public string User { get; set; } = string.Empty;

    /// <summary>The password used to authenticate with the SMTP server.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>The address the email is sent from.</summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>The display name the email is sent from.</summary>
    public string FromName { get; set; } = "ProManager Online";
}
