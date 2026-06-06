using ProManagerOnline.Site.Domain.Exceptions;

namespace ProManagerOnline.Site.Domain.Content;

/// <summary>
/// The site's "About" page: a single, editable piece of marketing content shown at <c>/about</c>
/// and maintained from the admin. The aggregate root for the About page. There is only ever one
/// instance, so it carries the well-known <see cref="AboutPageId.Single"/> identity. Its content is
/// a heading <see cref="Title"/> and a Markdown <see cref="Body"/>, both authored by the site owner.
/// </summary>
public sealed class AboutPage
{
    private AboutPage(AboutPageId id, string title, string body)
    {
        Id = id;
        Title = title;
        Body = body;
    }

    /// <summary>The page's identity; always <see cref="AboutPageId.Single"/>.</summary>
    public AboutPageId Id { get; }

    /// <summary>The page's heading.</summary>
    public string Title { get; private set; }

    /// <summary>The page body, authored in Markdown.</summary>
    public string Body { get; private set; }

    /// <summary>Creates the About page with its initial content.</summary>
    /// <param name="title">The page heading; must not be blank.</param>
    /// <param name="body">The Markdown body; must not be blank.</param>
    /// <returns>A new <see cref="AboutPage"/> carrying the singleton identity.</returns>
    /// <exception cref="DomainException">Thrown when the title or body is blank.</exception>
    public static AboutPage Create(string title, string body)
    {
        Require(title, nameof(title));
        Require(body, nameof(body));

        return new AboutPage(AboutPageId.Single, title.Trim(), body.Trim());
    }

    /// <summary>Replaces the page's heading and body.</summary>
    /// <param name="title">The new heading; must not be blank.</param>
    /// <param name="body">The new Markdown body; must not be blank.</param>
    /// <exception cref="DomainException">Thrown when the title or body is blank.</exception>
    public void UpdateContent(string title, string body)
    {
        Require(title, nameof(title));
        Require(body, nameof(body));

        Title = title.Trim();
        Body = body.Trim();
    }

    private static void Require(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"The About page's {field} must not be blank.");
        }
    }
}
