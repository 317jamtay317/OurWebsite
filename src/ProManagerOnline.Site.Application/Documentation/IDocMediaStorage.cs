namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>
/// Port for storing documentation screenshots. Implemented in the web layer (which knows where
/// static files are served from) and used by the authoring admin's upload flow. The returned URL
/// is what gets embedded in an article's Markdown, for example
/// <c>![caption](/docs-media/{productId}/{file}.png)</c>, where the reader already renders images.
/// </summary>
public interface IDocMediaStorage
{
    /// <summary>Saves an uploaded screenshot for a product's documentation.</summary>
    /// <param name="productId">The product the screenshot documents; used to group stored files.</param>
    /// <param name="fileName">The uploaded file's name, used to derive a safe name and extension.</param>
    /// <param name="content">The image content to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The site-relative URL of the stored image, ready to embed in Markdown.</returns>
    Task<string> SaveScreenshotAsync(
        Guid productId, string fileName, Stream content, CancellationToken cancellationToken = default);
}
