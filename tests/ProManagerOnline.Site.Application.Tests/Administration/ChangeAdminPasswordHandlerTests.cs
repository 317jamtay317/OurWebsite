using ProManagerOnline.Site.Application.Administration;
using ProManagerOnline.Site.Application.Tests.Fakes;
using Xunit;

namespace ProManagerOnline.Site.Application.Tests.Administration;

/// <summary>Behaviour of the <see cref="ChangeAdminPasswordHandler"/> use case.</summary>
public class ChangeAdminPasswordHandlerTests
{
    [Fact]
    public async Task Handle_PassesCredentialsToServiceAndReturnsItsResult()
    {
        var accounts = new FakeAdminAccountService { ChangeResult = AdminAccountResult.Success() };
        var handler = new ChangeAdminPasswordHandler(accounts);

        var result = await handler.Handle(
            new ChangeAdminPasswordCommand("admin-1", "old-pw", "new-pw"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(("admin-1", "old-pw", "new-pw"), accounts.LastChange);
    }

    [Fact]
    public async Task Handle_WhenServiceFails_ReturnsFailureWithErrors()
    {
        var accounts = new FakeAdminAccountService { ChangeResult = AdminAccountResult.Failure("Wrong password.") };
        var handler = new ChangeAdminPasswordHandler(accounts);

        var result = await handler.Handle(
            new ChangeAdminPasswordCommand("admin-1", "bad", "new-pw"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Wrong password.", result.Errors);
    }
}
