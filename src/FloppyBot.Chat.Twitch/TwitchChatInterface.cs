using System.Text.Json;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Chat.Twitch.Api;
using FloppyBot.Chat.Twitch.Config;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Chat.Twitch.Extensions;
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
    private readonly ChannelIdentifier _channelIdentifier;
    private readonly ITwitchClient _client;
    private readonly ILogger<TwitchClient> _clientLogger;
    private readonly TwitchConfiguration _configuration;

    private readonly ILogger<TwitchChatInterface> _logger;
    private readonly ITwitchChannelOnlineMonitor _onlineMonitor;
    private readonly ITwitchApiService _twitchApiService;

    public TwitchChatInterface(
        ILogger<TwitchChatInterface> logger,
        // ReSharper disable once ContextualLoggerProblem
        ILogger<TwitchClient> clientLogger,
        ITwitchClient client,
        TwitchConfiguration configuration,
        ITwitchChannelOnlineMonitor onlineMonitor,
        ITwitchApiService twitchApiService
    )
    {
        _logger = logger;
        _clientLogger = clientLogger;
        _configuration = configuration;
        _twitchApiService = twitchApiService;

        _onlineMonitor = onlineMonitor;
        _onlineMonitor.OnlineStatusChanged += OnlineMonitor_OnlineStatusChanged;

        _channelIdentifier = new ChannelIdentifier(IF_NAME, _configuration.Channel);

        _client = client;
        _client.OnLog += Client_OnLog;
        _client.OnConnected += Client_OnConnected;
        _client.OnConnectionError += Client_OnConnectionError;
        _client.OnJoinedChannel += Client_OnJoinedChannel;
        _client.OnMessageReceived += Client_OnMessageReceived;
        _client.OnReconnected += Client_OnReconnected;

        _client.OnUserJoined += Client_OnUserJoined;
        _client.OnUserLeft += Client_OnUserLeft;

        _client.OnNewSubscriber += Client_OnNewSubscriber;
        _client.OnReSubscriber += Client_OnReSubscriber;
        _client.OnGiftedSubscription += Client_OnGiftedSubscription;
        _client.OnCommunitySubscription += Client_OnCommunitySubscription;
        _client.OnRaidNotification += Client_OnRaidNotification;
    }

    public string Name => _channelIdentifier;
    public ChatInterfaceFeatures SupportedFeatures => ChatInterfaceFeatures.None;

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

    public void SendMessage(ChatMessageIdentifier referenceMessage, string message)
    {
        if (referenceMessage.IsNewMessage)
        {
            _logger.LogDebug("Message is not linked to a request, sending it as a new one instead");
            SendMessage(message);
            return;
        }

        _logger.LogTrace(
            "Sending reply message to channel {TwitchChannel} refering to message {ReplyMessageId}: {ChatMessage}",
            referenceMessage.Channel,
            referenceMessage.ToString(),
            message
        );
        LineSplitter(
            message,
            line => _client.SendReply(referenceMessage.Channel, referenceMessage.MessageId, line)
        );
    }

    public event ChatMessageReceivedDelegate? MessageReceived;

    public void Dispose()
    {
        _logger.LogTrace("Disposing interface ...");
        Disconnect();
    }

    public void SendMessage(string message)
    {
        SendMessage(_channelIdentifier, message);
    }

    public void SendMessage(ChannelIdentifier channel, string message)
    {
        var twitchChannel = channel.Channel.ToLowerInvariant();
        _logger.LogTrace(
            "Sending message to channel {TwitchChannel}: {ChatMessage}",
            twitchChannel,
            message
        );
        LineSplitter(message, line => _client.SendMessage(twitchChannel, line));
    }

    private static PrivilegeLevel DeterminePrivilegeLevel(
        TwitchLib.Client.Models.ChatMessage chatMessage
    )
    {
        return DeterminePrivilegeLevel(
            chatMessage.IsBroadcaster,
            chatMessage.IsModerator,
            chatMessage.IsMe
        );
    }

    private static void LineSplitter(
        string message,
        Action<string> lineAction,
        int sleepTimeMs = 5_000
    )
    {
        var lines = message.Split("\n\n");
        var currentLineIndex = 0;
        var currentLine = lines[currentLineIndex];

        while (currentLineIndex < lines.Length)
        {
            lineAction(currentLine);

            if (currentLineIndex + 1 >= lines.Length)
            {
                break;
            }

            currentLineIndex++;
            currentLine = lines[currentLineIndex];
            Thread.Sleep(sleepTimeMs);
        }
    }

    private static PrivilegeLevel DeterminePrivilegeLevel(
        bool isBroadcaster,
        bool isModerator,
        bool isMe
    )
    {
        if (isBroadcaster)
        {
            return PrivilegeLevel.Administrator;
        }

        if (isModerator)
        {
            return PrivilegeLevel.Moderator;
        }

        if (isMe)
        {
            return PrivilegeLevel.Unknown;
        }

        return PrivilegeLevel.Viewer;
    }

    private ChatMessageIdentifier NewChatMessageIdentifier(string? messageId)
    {
        return new ChatMessageIdentifier(
            IF_NAME,
            _configuration.Channel,
            messageId ?? Guid.NewGuid().ToString()
        );
    }

    private void Client_OnLog(object? _, OnLogArgs e)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        _clientLogger.LogTrace(e.Data);
    }

    private void Client_OnConnectionError(object? sender, OnConnectionErrorArgs e)
    {
        _logger.LogError("Connection Error occurred: {ErrorMessage}", e.Error.Message);
    }

    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        _logger.LogInformation(
            "Connected to Twitch using {BotUsername} (auto-joining to {AutoJoinChannel})",
            e.BotUsername,
            e.AutoJoinChannel
        );
    }

    private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        _logger.LogInformation("Joined channel {TwitchChannel}", e.Channel);
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        TwitchLib.Client.Models.ChatMessage? chatMessage = e.ChatMessage;

        _logger.LogTrace(
            "Received message from {TwitchUser}@{TwitchChannel}: {TwitchMessage}",
            e.ChatMessage.Username,
            chatMessage.Channel,
            chatMessage.Message
        );

        if (_configuration.DisableWhenChannelIsOffline && !_onlineMonitor.IsChannelOnline())
        {
            return;
        }

        var message = new ChatMessage(
            NewChatMessageIdentifier(e.ChatMessage.Id),
            TwitchEntityExtensions.ConvertToChatUser(
                chatMessage.Username,
                chatMessage.DisplayName,
                DeterminePrivilegeLevel(chatMessage)
            ),
            SharedEventTypes.CHAT_MESSAGE,
            chatMessage.Message,
            null,
            SupportedFeatures
        );

        MessageReceived?.Invoke(this, message);
    }

    private void Client_OnNewSubscriber(object? sender, OnNewSubscriberArgs e)
    {
        _logger.LogTrace(
            "Received new subscriber {TwitchUser}@{TwitchChannel}: {TwitchSubscriptionPlan}",
            e.Subscriber.DisplayName,
            e.Subscriber.Channel,
            e.Subscriber.SubscriptionPlan
        );

        var eventArgs = new TwitchSubscriptionReceivedEvent(
            e.Subscriber.SubscriptionPlan.ConvertToPlan(e.Subscriber.SubscriptionPlanName)
        );
        MessageReceived?.Invoke(
            this,
            new ChatMessage(
                NewChatMessageIdentifier(e.Subscriber.Id),
                TwitchEntityExtensions.ConvertToChatUser(
                    e.Subscriber.UserId,
                    e.Subscriber.DisplayName,
                    DeterminePrivilegeLevel(false, e.Subscriber.IsModerator, false)
                ),
                TwitchEventTypes.SUBSCRIPTION,
                JsonSerializer.Serialize(eventArgs),
                null,
                SupportedFeatures
            )
        );
    }

    private void Client_OnReSubscriber(object? sender, OnReSubscriberArgs e)
    {
        _logger.LogTrace(
            "Received re-subscriber {TwitchUser}@{TwitchChannel}: {TwitchSubscriptionPlan} for {TwitchSubscriptionMonths} months",
            e.ReSubscriber.DisplayName,
            e.Channel,
            e.ReSubscriber.SubscriptionPlan,
            e.ReSubscriber.Months
        );

        var eventArgs = new TwitchReSubscriptionReceivedEvent(
            e.ReSubscriber.SubscriptionPlan.ConvertToPlan(e.ReSubscriber.SubscriptionPlanName),
            e.ReSubscriber.Months
        );
        MessageReceived?.Invoke(
            this,
            new ChatMessage(
                NewChatMessageIdentifier(e.ReSubscriber.Id),
                TwitchEntityExtensions.ConvertToChatUser(
                    e.ReSubscriber.UserId,
                    e.ReSubscriber.DisplayName,
                    DeterminePrivilegeLevel(false, e.ReSubscriber.IsModerator, false)
                ),
                TwitchEventTypes.RE_SUBSCRIPTION,
                JsonSerializer.Serialize(eventArgs),
                null,
                SupportedFeatures
            )
        );
    }

    private void Client_OnGiftedSubscription(object? sender, OnGiftedSubscriptionArgs e)
    {
        _logger.LogTrace(
            "Received gifted subscription {TwitchUser}@{TwitchChannel}: {TwitchSubscriptionPlan} to {TwitchGiftRecipient} for {TwitchSubscriptionMonths} months",
            e.GiftedSubscription.DisplayName,
            e.Channel,
            e.GiftedSubscription.MsgParamSubPlan,
            e.GiftedSubscription.MsgParamRecipientUserName,
            e.GiftedSubscription.MsgParamMonths
        );

        var eventArgs = new TwitchSubscriptionGiftEvent(
            e.GiftedSubscription.MsgParamSubPlan.ConvertToPlan(
                e.GiftedSubscription.MsgParamSubPlanName
            ),
            TwitchEntityExtensions.ConvertToChatUser(
                e.GiftedSubscription.MsgParamRecipientUserName,
                e.GiftedSubscription.MsgParamRecipientDisplayName
            )
        );
        MessageReceived?.Invoke(
            this,
            new ChatMessage(
                NewChatMessageIdentifier(e.GiftedSubscription.Id),
                TwitchEntityExtensions.ConvertToChatUser(
                    e.GiftedSubscription.UserId,
                    e.GiftedSubscription.DisplayName,
                    DeterminePrivilegeLevel(false, e.GiftedSubscription.IsModerator, false)
                ),
                TwitchEventTypes.RE_SUBSCRIPTION,
                JsonSerializer.Serialize(eventArgs),
                null,
                SupportedFeatures
            )
        );
    }

    private void Client_OnCommunitySubscription(object? sender, OnCommunitySubscriptionArgs e)
    {
        _logger.LogTrace(
            "Received community subscription {TwitchUser}@{TwitchChannel}: User gifted {TwitchMassGiftCount} {TwitchSubscriptionPlan} subscriptions",
            e.GiftedSubscription.DisplayName,
            e.Channel,
            e.GiftedSubscription.MsgParamMassGiftCount,
            e.GiftedSubscription.MsgParamSubPlan
        );

        var eventArgs = new TwitchSubscriptionCommunityGiftEvent(
            e.GiftedSubscription.MsgParamSubPlan.ConvertToPlan(),
            e.GiftedSubscription.MsgParamMassGiftCount,
            e.GiftedSubscription.MsgParamMultiMonthGiftDuration
        );
        MessageReceived?.Invoke(
            this,
            new ChatMessage(
                NewChatMessageIdentifier(e.GiftedSubscription.Id),
                TwitchEntityExtensions.ConvertToChatUser(
                    e.GiftedSubscription.UserId,
                    e.GiftedSubscription.DisplayName,
                    DeterminePrivilegeLevel(false, e.GiftedSubscription.IsModerator, false)
                ),
                TwitchEventTypes.RE_SUBSCRIPTION,
                JsonSerializer.Serialize(eventArgs),
                null,
                SupportedFeatures
            )
        );
    }

    private void Client_OnRaidNotification(object? sender, OnRaidNotificationArgs e)
    {
        _logger.LogTrace(
            "Raid inbound from {TwitchUser}@{TwitchChannel}: {TwitchRaidViewerCount} viewers",
            e.RaidNotification.DisplayName,
            e.Channel,
            e.RaidNotification.MsgParamViewerCount
        );

        var eventArgs = new TwitchRaidEvent(
            e.RaidNotification.MsgParamLogin,
            e.RaidNotification.MsgParamDisplayName,
            e.RaidNotification.MsgParamViewerCount.ParseInt(),
            TryExtensions.TryOr(
                () =>
                {
                    var response = _twitchApiService.GetStreamTeamsOfChannel(
                        e.RaidNotification.MsgParamLogin
                    );
                    return response
                        .Select(t => new StreamTeam(t.Name, t.DisplayName))
                        .FirstOrDefault();
                },
                (ex) =>
                {
                    _logger.LogError(
                        ex,
                        "Failed to retrieve stream team information for {RaidChannelName}, returning null as default",
                        e.RaidNotification.MsgParamLogin
                    );
                    return null;
                }
            )
        );
        MessageReceived?.Invoke(
            this,
            new ChatMessage(
                NewChatMessageIdentifier(e.RaidNotification.Id),
                TwitchEntityExtensions.ConvertToChatUser(
                    e.RaidNotification.MsgParamLogin,
                    e.RaidNotification.MsgParamDisplayName,
                    DeterminePrivilegeLevel(false, false, false)
                ),
                TwitchEventTypes.RAID,
                JsonSerializer.Serialize(eventArgs),
                null,
                SupportedFeatures
            )
        );
    }

    private void Client_OnReconnected(object? sender, OnReconnectedEventArgs e)
    {
        _logger.LogInformation("Reconnected");
    }

    private void Client_OnUserJoined(object? sender, OnUserJoinedArgs e)
    {
        _logger.LogInformation(
            "User {TwitchUser} joined the channel {TwitchChannel}",
            e.Username,
            e.Channel
        );
        var eventArgs = new TwitchUserJoinedEvent(e.Channel, e.Username);
        MessageReceived?.Invoke(
            this,
            new ChatMessage(
                NewChatMessageIdentifier(null),
                TwitchEntityExtensions.ConvertToChatUser(
                    e.Username,
                    e.Username,
                    DeterminePrivilegeLevel(false, false, false)
                ),
                TwitchEventTypes.USER_JOINED,
                JsonSerializer.Serialize(eventArgs),
                null,
                SupportedFeatures
            )
        );
    }

    private void Client_OnUserLeft(object? sender, OnUserLeftArgs e)
    {
        _logger.LogInformation(
            "User {TwitchUser} left the channel {TwitchChannel}",
            e.Username,
            e.Channel
        );
        var eventArgs = new TwitchUserLeftEvent(e.Channel, e.Username);
        MessageReceived?.Invoke(
            this,
            new ChatMessage(
                NewChatMessageIdentifier(null),
                TwitchEntityExtensions.ConvertToChatUser(
                    e.Username,
                    e.Username,
                    DeterminePrivilegeLevel(false, false, false)
                ),
                TwitchEventTypes.USER_LEFT,
                JsonSerializer.Serialize(eventArgs),
                null,
                SupportedFeatures
            )
        );
    }

    private void OnlineMonitor_OnlineStatusChanged(
        ITwitchChannelOnlineMonitor sender,
        TwitchChannelOnlineStatusChangedEventArgs e
    )
    {
        _logger.LogTrace(
            "Channel online status changed to {IsChannelOnline}: {TwitchStream}",
            e.IsOnline,
            e.Stream
        );

        if (e.Stream is null || !_configuration.AnnounceChannelOnlineStatus)
        {
            return;
        }

        var channelId = new ChannelIdentifier(IF_NAME, e.Stream.ChannelName);
        SendMessage(
            channelId,
            e.IsOnline ? Resources.ChannelOnlineMessage : Resources.ChannelOfflineMessage
        );
    }
}
