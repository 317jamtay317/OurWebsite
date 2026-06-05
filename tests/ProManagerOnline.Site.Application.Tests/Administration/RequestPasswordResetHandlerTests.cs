using ProManagerOnline.Site.Application.Administration;
using ProManagerOnline.Site.Application.Tests.Fakes;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Administration;

/// <summary>Behaviour of the <see cref="RequestPasswordResetHandler"/> use case.</summary>
public class RequestPasswordResetHandlerTests
{
    [Fact]
    public async Task Handle_WhenAdminExists_SendsResetEmailContainingTheBuiltLink()
    {
        var accounts = new FakeAdminAccountService { ResetTokenToReturn = "token-123" };
        var emails = new FakeEmailSender();
        var handler = new RequestPasswordResetHandler(accounts, emails);

        await handler.Handle(
            new RequestPasswordResetCommand("owner@example.com"),
            token => $"https://site.example/reset?t={token}",
            CancellationToken.None);

        var sent = Assert.Single(emails.Sent);
        Assert.Equal("owner@example.com", sent.Recipient);
        Assert.Contains("https://site.example/reset?t=token-123", sent.HtmlBody);
        Assert.Equal("owner@example.com", accounts.LastGenerateTokenEmail);
    }

    [Fact]
    public async Task Handle_WhenAdminUnknown_SendsNoEmail()
    {
        var accounts = new FakeAdminAccountService { ResetTokenToReturn = null };
        var emails = new FakeEmailSender();
        var handler = new RequestPasswordResetHandler(accounts, emails);

        await handler.Handle(
            new RequestPasswordResetCommand("ghost@example.com"),
            token => $"https://site.example/reset?t={token}",
            CancellationToken.None);

        Assert.Empty(emails.Sent);
    }
}
