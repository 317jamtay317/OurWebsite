using ProManagerOnline.Site.Application.Administration;
using ProManagerOnline.Site.Application.Tests.Fakes;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Administration;

/// <summary>Behaviour of the <see cref="ResetAdminPasswordHandler"/> use case.</summary>
public class ResetAdminPasswordHandlerTests
{
    [Fact]
    public async Task Handle_PassesTokenAndPasswordToServiceAndReturnsResult()
    {
        var accounts = new FakeAdminAccountService { ResetResult = AdminAccountResult.Success() };
        var handler = new ResetAdminPasswordHandler(accounts);

        var result = await handler.Handle(
            new ResetAdminPasswordCommand("owner@example.com", "token-123", "new-pw"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(("owner@example.com", "token-123", "new-pw"), accounts.LastReset);
    }

    [Fact]
    public async Task Handle_WhenServiceFails_ReturnsFailureWithErrors()
    {
        var accounts = new FakeAdminAccountService { ResetResult = AdminAccountResult.Failure("Invalid token.") };
        var handler = new ResetAdminPasswordHandler(accounts);

        var result = await handler.Handle(
            new ResetAdminPasswordCommand("owner@example.com", "bad", "new-pw"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Invalid token.", result.Errors);
    }
}
