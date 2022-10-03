using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Chat.Twitch.Config;
using FloppyBot.Chat.Twitch.Monitor;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Communication.Events;

namespace FloppyBot.Chat.Twitch;

public class TwitchChatInterface : IChatInterface
{
    public const string IF_NAME = "Twitch";

    private readonly ILogger<TwitchChatInterface> _logger;
    private readonly ILogger<TwitchClient> _clientLogger;
    private readonly ITwitchClient _client;
    private readonly TwitchConfiguration _configuration;
    private readonly ChannelIdentifier _channelIdentifier;
    private readonly ITwitchChannelOnlineMonitor _onlineMonitor;

    public TwitchChatInterface(
        ILogger<TwitchChatInterface> logger,
        // ReSharper disable once ContextualLoggerProblem
        ILogger<TwitchClient> clientLogger,
        ITwitchClient client,
        TwitchConfiguration configuration,
        ITwitchChannelOnlineMonitor onlineMonitor)
    {
        _logger = logger;
        _clientLogger = clientLogger;
        _configuration = configuration;
        _onlineMonitor = onlineMonitor;

        _channelIdentifier = new ChannelIdentifier(IF_NAME, _configuration.Channel);

        _client = client;
        _client.OnLog += Client_OnLog;
        _client.OnConnected += Client_OnConnected;
        _client.OnConnectionError += Client_OnConnectionError;
        _client.OnJoinedChannel += Client_OnJoinedChannel;
        _client.OnMessageReceived += Client_OnMessageReceived;
        _client.OnReconnected += Client_OnReconnected;
    }

    public string Name => _channelIdentifier;
    public ChatInterfaceFeatures SupportedFeatures => ChatInterfaceFeatures.None;

    private ChatMessageIdentifier NewChatMessageIdentifier()
    {
        return new ChatMessageIdentifier(
            IF_NAME,
            _configuration.Channel,
            Guid.NewGuid().ToString());
    }

    public void Connect()
    {
        _logger.LogInformation("Connecting to Twitch ...");
        if (!_client.Connect())
        {
            _logger.LogCritical("Failed to connect Twitch client!");
        }
    }

    public void Disconnect()
    {
        if (_client.IsConnected)
        {
            _logger.LogInformation("Disconnecting from Twitch ...");
            _client.Disconnect();
        }
    }

    public void SendMessage(string message)
    {
        SendMessage(_channelIdentifier, message);
    }

    public void SendMessage(ChannelIdentifier channel, string message)
    {
        string twitchChannel = channel.Channel.ToLowerInvariant();
        foreach (string line in message.Split("\n\n"))
        {
            _client.SendMessage(twitchChannel, line);
        }
    }

    public event ChatMessageReceivedDelegate? MessageReceived;

    public void Dispose()
    {
        _logger.LogTrace("Disposing interface ...");
        Disconnect();
    }

    private void Client_OnLog(object? _, OnLogArgs e)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        _clientLogger.LogTrace(e.Data);
    }

    private void Client_OnConnectionError(object? sender, OnConnectionErrorArgs e)
    {
        _logger.LogError(
            "Connection Error occurred: {ErrorMessage}",
            e.Error.Message);
    }

    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        _logger.LogInformation(
            "Connected to Twitch using {BotUsername} (auto-joining to {AutoJoinChannel})",
            e.BotUsername,
            e.AutoJoinChannel);
    }

    private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        _logger.LogInformation(
            "Joined channel {TwitchChannel}",
            e.Channel);
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        TwitchLib.Client.Models.ChatMessage? chatMessage = e.ChatMessage;

        _logger.LogTrace(
            "Received message from {TwitchUser}@{TwitchChannel}: {TwitchMessage}",
            e.ChatMessage.Username,
            chatMessage.Channel,
            chatMessage.Message);

        if (_configuration.DisableWhenChannelIsOffline
            && !_onlineMonitor.IsChannelOnline())
        {
            return;
        }

        var message = new ChatMessage(
            NewChatMessageIdentifier(),
            new ChatUser(
                new ChannelIdentifier(
                    IF_NAME,
                    chatMessage.Username),
                chatMessage.DisplayName,
                DeterminePrivilegeLevel(chatMessage)),
            SharedEventTypes.CHAT_MESSAGE,
            chatMessage.Message);

        MessageReceived?.Invoke(this, message);
    }

    private static PrivilegeLevel DeterminePrivilegeLevel(TwitchLib.Client.Models.ChatMessage chatMessage)
    {
        if (chatMessage.IsBroadcaster)
            return PrivilegeLevel.Administrator;
        if (chatMessage.IsModerator)
            return PrivilegeLevel.Moderator;
        if (chatMessage.IsMe)
            return PrivilegeLevel.Unknown;
        return PrivilegeLevel.Viewer;
    }

    private void Client_OnReconnected(object? sender, OnReconnectedEventArgs e)
    {
        _logger.LogInformation("Reconnected");
    }
}
