using System.Text.Json;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
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

    public TwitchChatInterface(
        ILogger<TwitchChatInterface> logger,
        // ReSharper disable once ContextualLoggerProblem
        ILogger<TwitchClient> clientLogger,
        ITwitchClient client,
        TwitchConfiguration configuration,
        ITwitchChannelOnlineMonitor onlineMonitor
    )
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

        _client.OnNewSubscriber += Client_OnNewSubscriber;
        _client.OnReSubscriber += Client_OnReSubscriber;
        _client.OnGiftedSubscription += Client_OnGiftedSubscription;
        _client.OnCommunitySubscription += Client_OnCommunitySubscription;
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
        _client.SendReply(referenceMessage.Channel, referenceMessage.MessageId, message);
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
        string twitchChannel = channel.Channel.ToLowerInvariant();
        foreach (string line in message.Split("\n\n"))
        {
            _client.SendMessage(twitchChannel, line);
        }
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

    private void Client_OnReconnected(object? sender, OnReconnectedEventArgs e)
    {
        _logger.LogInformation("Reconnected");
    }
}
