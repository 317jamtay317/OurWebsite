using ProManagerOnline.Site.Application.Contact;
using ProManagerOnline.Site.Application.Tests.Fakes;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Contact;

/// <summary>
/// Behaviour of the <see cref="SubmitContactInquiryHandler"/> use case: it gates the contact
/// form on a CAPTCHA check, and only emails the enquiry to the business inbox when it passes.
/// </summary>
public class SubmitContactInquiryHandlerTests
{
    private const string Inbox = "james@jaila-files.com";

    [Fact]
    public async Task Handle_WhenHuman_EmailsTheEnquiryToTheBusinessInbox()
    {
        var captcha = new FakeCaptchaValidator { Result = true };
        var emails = new FakeEmailSender();
        var handler = new SubmitContactInquiryHandler(captcha, emails);

        var outcome = await handler.Handle(SampleCommand(), Inbox, CancellationToken.None);

        Assert.Equal(ContactSubmissionOutcome.Delivered, outcome);
        var sent = Assert.Single(emails.Sent);
        Assert.Equal(Inbox, sent.Recipient);
        Assert.Contains("dana@acme.test", sent.HtmlBody);
        Assert.Contains("Can your platform handle our invoicing?", sent.HtmlBody);
        Assert.Contains("Workflows.AI", sent.Subject);
    }

    [Fact]
    public async Task Handle_PassesTheCaptchaResponseAndIpToTheValidator()
    {
        var captcha = new FakeCaptchaValidator { Result = true };
        var handler = new SubmitContactInquiryHandler(captcha, new FakeEmailSender());

        await handler.Handle(SampleCommand(), Inbox, CancellationToken.None);

        Assert.Equal("captcha-token", captcha.LastResponse);
        Assert.Equal("203.0.113.5", captcha.LastRemoteIp);
    }

    [Fact]
    public async Task Handle_WhenCaptchaFails_SendsNoEmailAndReportsSpam()
    {
        var captcha = new FakeCaptchaValidator { Result = false };
        var emails = new FakeEmailSender();
        var handler = new SubmitContactInquiryHandler(captcha, emails);

        var outcome = await handler.Handle(SampleCommand(), Inbox, CancellationToken.None);

        Assert.Equal(ContactSubmissionOutcome.RejectedAsSpam, outcome);
        Assert.Empty(emails.Sent);
    }

    private static SubmitContactInquiryCommand SampleCommand() =>
        new(
            Name: "Dana Lee",
            Email: "dana@acme.test",
            Company: "Acme Co",
            ProductOfInterest: "Workflows.AI",
            Message: "Can your platform handle our invoicing?",
            CaptchaResponse: "captcha-token",
            RemoteIp: "203.0.113.5");
}
