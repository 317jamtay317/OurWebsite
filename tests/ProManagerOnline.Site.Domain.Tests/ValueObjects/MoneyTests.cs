using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Domain.Tests.ValueObjects;

/// <summary>
/// Behaviour of the <see cref="Money"/> value object: a monetary amount in a
/// single currency, used for product plan prices.
/// </summary>
public class MoneyTests
{
    [Fact]
    public void Create_GivenAmountAndCurrency_ExposesThem()
    {
        // Arrange / Act
        var price = Money.Create(29m, Currency.Usd);

        // Assert
        Assert.Equal(29m, price.Amount);
        Assert.Equal(Currency.Usd, price.Currency);
    }

    [Fact]
    public void Create_GivenZeroAmount_IsAllowed()
    {
        // A free plan is a legitimate price of zero.
        var price = Money.Create(0m, Currency.Usd);

        Assert.Equal(0m, price.Amount);
    }

    [Fact]
    public void Create_GivenNegativeAmount_ThrowsDomainException()
    {
        // A price can never be negative.
        Assert.Throws<DomainException>(() => Money.Create(-0.01m, Currency.Usd));
    }

    [Fact]
    public void TwoAmounts_WithSameValueAndCurrency_AreEqual()
    {
        Assert.Equal(Money.Create(79m, Currency.Usd), Money.Create(79m, Currency.Usd));
    }

    [Fact]
    public void TwoAmounts_WithDifferentValue_AreNotEqual()
    {
        Assert.NotEqual(Money.Create(79m, Currency.Usd), Money.Create(149m, Currency.Usd));
    }
}
