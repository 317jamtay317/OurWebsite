using ProManagerOnline.Site.Domain.Exceptions;
using ProManagerOnline.Site.Domain.ValueObjects;
using Xunit;

namespace ProManagerOnline.Site.Domain.Tests.ValueObjects;

/// <summary>
/// Behaviour of the <see cref="Slug"/> value object: a URL-safe identifier used in
/// product and documentation links (for example <c>air-compliance</c>).
/// </summary>
public class SlugTests
{
    [Theory]
    [InlineData("workflows")]
    [InlineData("air-compliance")]
    [InlineData("plan-2")]
    public void Create_GivenValidSlug_ExposesValue(string value)
    {
        var slug = Slug.Create(value);

        Assert.Equal(value, slug.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Workflows")]          // uppercase not allowed
    [InlineData("air compliance")]     // spaces not allowed
    [InlineData("air_compliance")]     // underscores not allowed
    [InlineData("-leading")]           // no leading hyphen
    [InlineData("trailing-")]          // no trailing hyphen
    [InlineData("double--hyphen")]     // no consecutive hyphens
    public void Create_GivenInvalidSlug_ThrowsDomainException(string value)
    {
        Assert.Throws<DomainException>(() => Slug.Create(value));
    }

    [Fact]
    public void TwoSlugs_WithSameValue_AreEqual()
    {
        Assert.Equal(Slug.Create("workflows"), Slug.Create("workflows"));
    }
}
