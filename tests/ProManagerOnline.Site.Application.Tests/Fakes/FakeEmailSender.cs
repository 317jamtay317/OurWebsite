using ProManagerOnline.Site.Application.Email;

namespace ProManagerOnline.Site.Application.Tests.Fakes;

/// <summary>A captured email, recorded by <see cref="FakeEmailSender"/>.</summary>
internal sealed record SentEmail(string Recipient, string Subject, string HtmlBody);

/// <summary>
/// In-memory <see cref="IEmailSender"/> test double that records every message sent,
/// so handler tests can assert what (if anything) was emailed.
/// </summary>
internal sealed class FakeEmailSender : IEmailSender
{
    public List<SentEmail> Sent { get; } = [];

    public Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        Sent.Add(new SentEmail(recipient, subject, htmlBody));
        return Task.CompletedTask;
    }
}
