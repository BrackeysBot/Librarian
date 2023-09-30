using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace Librarian.Services;

internal sealed class BookmarkService : BackgroundService
{
    private readonly DiscordSocketClient _discordClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BookmarkService" /> class.
    /// </summary>
    /// <param name="discordClient">The Discord client.</param>
    public BookmarkService(DiscordSocketClient discordClient)
    {
        _discordClient = discordClient;
    }

    /// <summary>
    ///     Creates a bookmark for the specified message.
    /// </summary>
    /// <param name="user">The user who bookmarked the message.</param>
    /// <param name="message">The message to bookmark.</param>
    /// <returns>The bookmark sent to the user.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="user" /> or <paramref name="message" /> is <see langword="null" />.
    /// </exception>
    public async Task<IUserMessage?> CreateBookmarkAsync(IGuildUser user, IUserMessage message)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        if (message is null) throw new ArgumentNullException(nameof(message));

        IDMChannel dmChannel;
        try
        {
            dmChannel = await user.CreateDMChannelAsync();
        }
        catch
        {
            return null;
        }

        IGuild guild = user.Guild;

        var embed = new EmbedBuilder();
        embed.WithTitle("🔖 Bookmarked Message");
        embed.WithDescription($"You bookmarked a message in **{guild.Name}**.");
        embed.WithColor(Color.Green);
        embed.WithTimestamp(DateTimeOffset.UtcNow);
        embed.WithThumbnailUrl(guild.IconUrl);
        embed.AddField("Author", message.Author.Mention, true);
        embed.AddField("Sent", $"<t:{message.Timestamp.ToUnixTimeSeconds()}:R>", true);
        embed.AddField("Channel", MentionUtils.MentionChannel(message.Channel.Id), true);

        var jumpButton = new ButtonBuilder();
        jumpButton.WithLabel("Jump to Message");
        jumpButton.WithStyle(ButtonStyle.Link);
        jumpButton.WithUrl(message.GetJumpUrl());

        var deleteButton = new ButtonBuilder();
        deleteButton.WithLabel("Delete Bookmark");
        deleteButton.WithStyle(ButtonStyle.Danger);
        deleteButton.WithCustomId("delete_bookmark");
        deleteButton.WithEmote(new Emoji("🗑️"));

        var components = new ComponentBuilder();
        components.WithButton(jumpButton);
        components.WithButton(deleteButton);

        return await dmChannel.SendMessageAsync(embed: embed.Build(), components: components.Build()).ConfigureAwait(false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordClient.ButtonExecuted += OnDeleteBookmark;
        return Task.CompletedTask;
    }

    private async Task OnDeleteBookmark(SocketMessageComponent component)
    {
        if (component.Data.CustomId != "delete_bookmark")
        {
            return;
        }

        await component.Message.DeleteAsync().ConfigureAwait(false);
        await component.RespondAsync("Bookmark deleted!", ephemeral: true).ConfigureAwait(false);
    }
}
