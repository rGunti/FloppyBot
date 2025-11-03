using AwesomeAssertions.Extensions;
using Discord;
using Discord.WebSocket;
using FloppyBot.Chat.Discord;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Registry;
using FloppyBot.IntTest.Config;
using FloppyBot.IntTest.Utils;
using Microsoft.Extensions.Logging;
using LoggingUtils = FloppyBot.Base.Testing.LoggingUtils;

namespace FloppyBot.IntTest.Fixtures;

[CollectionDefinition(NAME)]
public class DiscordFixtureCollection : ICollectionFixture<DiscordFixture>
{
    public const string NAME = "Discord";
}

public class DiscordFixture : IAsyncDisposable
{
    private static readonly DiscordSocketConfig DiscordSocketConfig = new()
    {
        GatewayIntents =
            (
                GatewayIntents.AllUnprivileged
                & ~GatewayIntents.GuildScheduledEvents
                & ~GatewayIntents.GuildInvites
            ) | GatewayIntents.MessageContent,
    };

    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DiscordFixture> _logger;

    private readonly DiscordChatInterface _bot1;
    private readonly DiscordChatInterface _bot2;

    public DiscordFixture()
    {
        _loggerFactory = LoggingUtils.GetLoggerFactory();
        _logger = _loggerFactory.CreateLogger<DiscordFixture>();
        _logger.LogInformation("Starting Discord Fixture ...");

        var config = IntegrationTestingConfigurationUtils.BuildConfiguration();
        var testConfig = config.GetIntegrationTestingConfiguration();

        Bot1Id = testConfig.DiscordBot1.ClientId;
        Bot2Id = testConfig.DiscordBot2.ClientId;

        ServerId = testConfig.DiscordServerId;
        ChannelId = testConfig.DiscordChannelId;

        _bot1 = new DiscordChatInterface(
            _loggerFactory.CreateLogger<DiscordChatInterface>(),
            _loggerFactory.CreateLogger<DiscordSocketClient>(),
            testConfig.DiscordBot1,
            new DiscordSocketClient(DiscordSocketConfig),
            A.Fake<IDistributedCommandRegistry>()
        );
        _bot2 = new DiscordChatInterface(
            _loggerFactory.CreateLogger<DiscordChatInterface>(),
            _loggerFactory.CreateLogger<DiscordSocketClient>(),
            testConfig.DiscordBot2,
            new DiscordSocketClient(DiscordSocketConfig),
            A.Fake<IDistributedCommandRegistry>()
        );

        _bot1.Connect();
        _bot2.Connect();
    }

    public bool IsConnected => _bot1.IsConnected && _bot2.IsConnected;

    public DiscordChatInterface Bot1 => _bot1;
    public string Bot1Id { get; }

    public DiscordChatInterface Bot2 => _bot2;
    public string Bot2Id { get; }

    public string ServerId { get; }

    public string ChannelId { get; }

    public ChatMessageIdentifier DefaultChannelIdentifier =>
        ChatMessageIdentifier.NewFor($"{DiscordChatInterface.IF_NAME}/{ChannelId}");

    public async Task WaitForConnection()
    {
        await Wait.Until(() => IsConnected, 5.Seconds(), "Interfaces did not connect in time");
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing Discord Fixture ...");

        _bot1.Disconnect();
        _bot2.Disconnect();

        await Wait.Until(
            () => !_bot1.IsConnected && !_bot2.IsConnected,
            5.Seconds(),
            "Interfaces did not disconnect in time"
        );

        GC.SuppressFinalize(this);
    }
}
