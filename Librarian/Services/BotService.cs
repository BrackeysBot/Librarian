using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Librarian.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Librarian.Services;

/// <summary>
///     Represents a service which manages the bot's Discord connection.
/// </summary>
internal sealed class BotService : BackgroundService
{
    private readonly ILogger<BotService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly DiscordSocketClient _discordClient;
    private readonly InteractionService _interactionService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BotService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="discordClient">The Discord client.</param>
    /// <param name="interactionService">The interaction service.</param>
    public BotService(ILogger<BotService> logger,
        IServiceProvider serviceProvider,
        DiscordSocketClient discordClient,
        InteractionService interactionService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _discordClient = discordClient;
        _interactionService = interactionService;

        var attribute = typeof(BotService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        Version = attribute?.InformationalVersion ?? "Unknown";

        attribute = typeof(DiscordSocketClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        DiscordNetVersion = attribute?.InformationalVersion ?? "Unknown";
    }

    /// <summary>
    ///     Gets the Discord.Net version.
    /// </summary>
    /// <value>The Discord.Net version.</value>
    public string DiscordNetVersion { get; }

    /// <summary>
    ///     Gets the date and time at which the bot was started.
    /// </summary>
    /// <value>The start timestamp.</value>
    public DateTimeOffset StartedAt { get; private set; }

    /// <summary>
    ///     Gets the bot version.
    /// </summary>
    /// <value>The bot version.</value>
    public string Version { get; }

    /// <inheritdoc />
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(_discordClient.DisposeAsync().AsTask(), base.StopAsync(cancellationToken));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        StartedAt = DateTimeOffset.UtcNow;
        _logger.LogInformation("Librarian v{Version} is starting...", Version);

        await _interactionService.AddModuleAsync<BookmarkCommand>(_serviceProvider).ConfigureAwait(false);
        await _interactionService.AddModuleAsync<InfoCommand>(_serviceProvider).ConfigureAwait(false);

        _logger.LogInformation("Connecting to Discord...");
        _discordClient.Ready += OnReady;
        _discordClient.InteractionCreated += OnInteractionCreated;

        await _discordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN")).ConfigureAwait(false);
        await _discordClient.StartAsync().ConfigureAwait(false);
    }

    private async Task OnInteractionCreated(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_discordClient, interaction);
            IResult result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        break;
                }
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }

    private Task OnReady()
    {
        _logger.LogInformation("Discord client ready");
        return _interactionService.RegisterCommandsGloballyAsync();
    }
}
