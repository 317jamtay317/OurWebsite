using ProManagerOnline.Site.Application.Documentation;

namespace ProManagerOnline.Site.Web.Media;

/// <summary>
/// Stores documentation screenshots as static files under <c>wwwroot/docs-media/{productId}</c>,
/// so they are served directly by the static-files middleware and can be embedded in article
/// Markdown with a site-relative URL.
/// </summary>
/// <param name="environment">The hosting environment, used to locate the web root.</param>
public sealed class WwwrootDocMediaStorage(IWebHostEnvironment environment) : IDocMediaStorage
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg" };

    /// <inheritdoc />
    public async Task<string> SaveScreenshotAsync(
        Guid productId, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
        {
            extension = ".png";
        }

        // A fresh name avoids collisions and sanitises whatever the upload was called.
        var storedName = $"{Guid.NewGuid():n}{extension.ToLowerInvariant()}";
        var productFolder = productId.ToString("n");

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var directory = Path.Combine(webRoot, "docs-media", productFolder);
        Directory.CreateDirectory(directory);

        await using (var file = File.Create(Path.Combine(directory, storedName)))
        {
            await content.CopyToAsync(file, cancellationToken);
        }

        // URLs always use forward slashes, regardless of the OS path separator.
        return $"/docs-media/{productFolder}/{storedName}";
    }
}
