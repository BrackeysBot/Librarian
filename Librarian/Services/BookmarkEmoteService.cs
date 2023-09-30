using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace Librarian.Services;

/// <summary>
///     Represents a service which listens for the bookmark emote.
/// </summary>
internal sealed class BookmarkEmoteService : BackgroundService
{
    private readonly DiscordSocketClient _discordClient;
    private readonly BookmarkService _bookmarkService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BookmarkDeletionService" /> class.
    /// </summary>
    /// <param name="discordClient">The Discord client.</param>
    /// <param name="bookmarkService">The bookmark service.</param>
    public BookmarkEmoteService(DiscordSocketClient discordClient, BookmarkService bookmarkService)
    {
        _discordClient = discordClient;
        _bookmarkService = bookmarkService;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordClient.ReactionAdded += OnBookmarkAdded;
        return Task.CompletedTask;
    }

    private async Task OnBookmarkAdded(Cacheable<IUserMessage, ulong> message,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction)
    {
        var theChannel = (ITextChannel)(channel.HasValue ? channel.Value : reaction.Channel);
        var theMessage = (IUserMessage)(message.HasValue
                ? message.Value
                : await theChannel.GetMessageAsync(message.Id).ConfigureAwait(false)
            );

        if (theMessage.Channel is not ITextChannel) return; // ignore DMs
        if (reaction.Emote.Name != "🔖") return;

        IUser user = reaction.User.Value;

        await theMessage.RemoveReactionAsync(reaction.Emote, user).ConfigureAwait(false);
        await _bookmarkService.CreateBookmarkAsync((IGuildUser)user, theMessage).ConfigureAwait(false);
    }
}
