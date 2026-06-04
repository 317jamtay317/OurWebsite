using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ProManagerOnline.Site.Web.Mcp;

/// <summary>
/// A minimal Model Context Protocol tool that returns a hello-world greeting.
/// It exists to verify the MCP server is wired up and reachable; richer tools
/// wrapping the application's use-cases will be added later.
/// </summary>
[McpServerToolType]
public sealed class GreetingTool
{
    /// <summary>Returns a friendly hello-world greeting.</summary>
    /// <returns>The greeting text.</returns>
    [McpServerTool(Name = "say_hello")]
    [Description("Returns a friendly \"Hello, world!\" greeting.")]
    public string SayHello() => "Hello, world!";
}
