using ProManagerOnline.Site.Web.Mcp;

namespace ProManagerOnline.Site.Web.Tests.Mcp;

/// <summary>
/// Behaviour of the <see cref="GreetingTool"/> MCP tool: it returns the
/// hello-world greeting exposed over the Model Context Protocol endpoint.
/// </summary>
public class GreetingToolTests
{
    [Fact]
    public void SayHello_ReturnsHelloWorldGreeting()
    {
        // Arrange
        var tool = new GreetingTool();

        // Act
        var greeting = tool.SayHello();

        // Assert
        Assert.Equal("Hello, world!", greeting);
    }
}
