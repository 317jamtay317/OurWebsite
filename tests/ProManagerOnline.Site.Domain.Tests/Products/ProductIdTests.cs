using ProManagerOnline.Site.Domain.Products;
using Xunit;

namespace ProManagerOnline.Site.Domain.Tests.Products;

/// <summary>
/// Behaviour of the strongly-typed <see cref="ProductId"/> identifier.
/// </summary>
public class ProductIdTests
{
    [Fact]
    public void New_GeneratesNonEmptyId()
    {
        var id = ProductId.New();

        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_GeneratesUniqueIds()
    {
        Assert.NotEqual(ProductId.New(), ProductId.New());
    }

    [Fact]
    public void TwoIds_WrappingSameValue_AreEqual()
    {
        var value = Guid.NewGuid();

        Assert.Equal(new ProductId(value), new ProductId(value));
    }
}
