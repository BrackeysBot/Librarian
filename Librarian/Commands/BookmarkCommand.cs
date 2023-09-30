using Discord;
using Discord.Interactions;
using Librarian.Services;

namespace Librarian.Commands;

/// <summary>
///     Represents a class which implements the <c>Bookmark</c> context menu.
/// </summary>
internal sealed class BookmarkCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly BookmarkService _bookmarkService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BookmarkCommand" /> class.
    /// </summary>
    /// <param name="bookmarkService">The bookmark service.</param>
    public BookmarkCommand(BookmarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;
    }

    [MessageCommand("Bookmark")]
    [RequireContext(ContextType.Guild)]
    public async Task BookmarkAsync(IMessage message)
    {
        if (message is not IUserMessage userMessage)
        {
            return;
        }

        var member = (IGuildUser)Context.User;
        IUserMessage? bookmark = await _bookmarkService.CreateBookmarkAsync(member, userMessage).ConfigureAwait(false);
        if (bookmark is null)
        {
            await RespondAsync("Bookmark failed. Please make sure you have DMs enabled.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var response = $"Bookmark created. [Check your DMs]({bookmark.GetJumpUrl()}) to view your bookmarks.";
        await RespondAsync(response, ephemeral: true).ConfigureAwait(false);
    }
}
