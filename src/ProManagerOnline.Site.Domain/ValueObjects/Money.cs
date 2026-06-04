using ProManagerOnline.Site.Domain.Exceptions;

namespace ProManagerOnline.Site.Domain.ValueObjects;

/// <summary>
/// A monetary amount in a single <see cref="Currency"/>. A value object: immutable
/// and compared by value. Used for product plan prices, the amount is never negative.
/// </summary>
public sealed record Money
{
    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>The monetary amount. Always zero or greater.</summary>
    public decimal Amount { get; }

    /// <summary>The currency the <see cref="Amount"/> is expressed in.</summary>
    public Currency Currency { get; }

    /// <summary>
    /// Creates a <see cref="Money"/> value, enforcing that the amount is not negative.
    /// </summary>
    /// <param name="amount">The monetary amount; must be zero or greater. Zero represents a free plan.</param>
    /// <param name="currency">The currency the amount is expressed in.</param>
    /// <returns>A new <see cref="Money"/> instance.</returns>
    /// <exception cref="DomainException">Thrown when <paramref name="amount"/> is negative.</exception>
    public static Money Create(decimal amount, Currency currency)
    {
        if (amount < 0m)
        {
            throw new DomainException($"A price cannot be negative (was {amount}).");
        }

        return new Money(amount, currency);
    }
}
