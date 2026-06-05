using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Documentation;

namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>Input for editing an existing documentation article's content and position.</summary>
/// <param name="Id">The article to edit.</param>
/// <param name="Title">The new title.</param>
/// <param name="Section">The new section.</param>
/// <param name="Body">The new Markdown body.</param>
/// <param name="Position">The new order within the section; must not be negative.</param>
/// <remarks>An article's slug is fixed once created, so it is not part of an edit.</remarks>
public sealed record UpdateDocArticleCommand(Guid Id, string Title, string Section, string Body, int Position);

/// <summary>
/// Edits an existing documentation article's title, section, body and position. The article's
/// publication status is left unchanged — publishing is a separate use case.
/// </summary>
/// <param name="articles">The documentation-article repository.</param>
public sealed class UpdateDocArticleHandler(IDocArticleRepository articles)
{
    /// <summary>Applies the edit to the article.</summary>
    /// <param name="command">The new content and position.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no article has the given identifier.</exception>
    public async Task Handle(UpdateDocArticleCommand command, CancellationToken cancellationToken = default)
    {
        var article = await articles.GetByIdAsync(new DocArticleId(command.Id), cancellationToken)
            ?? throw new NotFoundException($"No documentation article exists with id {command.Id}.");

        article.UpdateContent(command.Title, command.Section, command.Body);
        article.Reposition(command.Position);

        await articles.SaveChangesAsync(cancellationToken);
    }
}
