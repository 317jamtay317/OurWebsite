using System.Text.RegularExpressions;
using ProManagerOnline.Site.Domain.Exceptions;

namespace ProManagerOnline.Site.Domain.ValueObjects;

/// <summary>
/// A URL-safe identifier used in product and documentation links (for example
/// <c>air-compliance</c>). A value object: immutable and compared by value. A valid
/// slug is one or more lowercase alphanumeric segments joined by single hyphens, with
/// no leading, trailing or consecutive hyphens.
/// </summary>
public sealed partial record Slug
{
    /// <summary>The greatest number of characters a slug may contain.</summary>
    public const int MaxLength = 100;

    private Slug(string value) => Value = value;

    /// <summary>The validated slug text.</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a <see cref="Slug"/> from already-canonical text, enforcing the slug format.
    /// </summary>
    /// <param name="value">The candidate slug, for example <c>air-compliance</c>.</param>
    /// <returns>A new, validated <see cref="Slug"/>.</returns>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="value"/> is null, blank, longer than <see cref="MaxLength"/>,
    /// or does not match the slug format.
    /// </exception>
    public static Slug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("A slug cannot be empty.");
        }

        if (value.Length > MaxLength)
        {
            throw new DomainException($"A slug cannot exceed {MaxLength} characters (was {value.Length}).");
        }

        if (!SlugPattern().IsMatch(value))
        {
            throw new DomainException(
                $"'{value}' is not a valid slug. Use lowercase letters, digits and single hyphens, e.g. 'air-compliance'.");
        }

        return new Slug(value);
    }

    /// <summary>Returns the slug text.</summary>
    /// <returns>The slug <see cref="Value"/>.</returns>
    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();
}
