using ProManagerOnline.Site.Application.Exceptions;
using ProManagerOnline.Site.Domain.Documentation;

namespace ProManagerOnline.Site.Application.Documentation;

/// <summary>Input for changing a documentation article's publication status.</summary>
/// <param name="Id">The article whose status to change.</param>
/// <param name="Publish">
/// <see langword="true"/> to publish the article (make it visible on the public site);
/// <see langword="false"/> to unpublish it (return it to draft and hide it).
/// </param>
public sealed record SetDocArticleStatusCommand(Guid Id, bool Publish);

/// <summary>Publishes or unpublishes a documentation article.</summary>
/// <param name="articles">The documentation-article repository.</param>
public sealed class SetDocArticleStatusHandler(IDocArticleRepository articles)
{
    /// <summary>Publishes or unpublishes the article according to the command.</summary>
    /// <param name="command">Which article to change, and the desired status.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no article has the given identifier.</exception>
    public async Task Handle(SetDocArticleStatusCommand command, CancellationToken cancellationToken = default)
    {
        var article = await articles.GetByIdAsync(new DocArticleId(command.Id), cancellationToken)
            ?? throw new NotFoundException($"No documentation article exists with id {command.Id}.");

        if (command.Publish)
        {
            article.Publish();
        }
        else
        {
            article.Unpublish();
        }

        await articles.SaveChangesAsync(cancellationToken);
    }
}
