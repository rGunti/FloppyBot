using System.Collections.Immutable;
using System.Text.Json;
using FloppyBot.Base.Configuration;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Commands.Aux.Currency.Storage;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Aux.Currency.BackgroundServices;

public class ChannelCurrencyMaintainer : BackgroundService
{
    private static readonly IImmutableSet<string> ChatMessageEvents =
    [
        TwitchEventTypes.USER_JOINED,
        TwitchEventTypes.USER_LEFT,
    ];

    private readonly ILogger<ChannelCurrencyMaintainer> _logger;
    private readonly INotificationReceiver<ChatMessage> _receiver;
    private readonly IChannelUserStateService _channelUserStateService;

    public ChannelCurrencyMaintainer(
        ILogger<ChannelCurrencyMaintainer> logger,
        INotificationReceiverFactory receiverFactory,
        IChannelUserStateService channelUserStateService,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _channelUserStateService = channelUserStateService;
        _receiver = receiverFactory.GetNewReceiver<ChatMessage>(
            configuration.GetParsedConnectionString("CurrencyMaintainer")
        );
        _receiver.NotificationReceived += OnNotificationReceived;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Channel Currency Maintainer ...");
        _receiver.StartListening();
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Channel Currency Maintainer ...");
        _receiver.StopListening();
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private static TwitchEvent ParseTwitchEvent(ChatMessage chatMessage)
    {
        return chatMessage.EventName switch
        {
            TwitchEventTypes.USER_JOINED
                => JsonSerializer.Deserialize<TwitchUserJoinedEvent>(chatMessage.Content)!,
            TwitchEventTypes.USER_LEFT
                => JsonSerializer.Deserialize<TwitchUserLeftEvent>(chatMessage.Content)!,
            _ => throw new InvalidOperationException($"Unknown event type {chatMessage.EventName}")
        };
    }

    private void OnNotificationReceived(ChatMessage chatMessage)
    {
#if DEBUG
        _logger.LogDebug("Received chat message {@ChatMessage}", chatMessage);
#endif
        if (!ChatMessageEvents.Contains(chatMessage.EventName))
        {
#if DEBUG
            _logger.LogDebug(
                "Chat message has event {EventName}, which is not relevant for this service, disregarding",
                chatMessage.EventName
            );
#endif
            return;
        }

        switch (ParseTwitchEvent(chatMessage))
        {
            case TwitchUserJoinedEvent joinedEvent:
                _logger.LogInformation(
                    "User {UserName} joined channel {ChannelName}",
                    joinedEvent.UserName,
                    joinedEvent.ChannelName
                );
                _channelUserStateService.MarkOnline(joinedEvent.UserName, joinedEvent.ChannelName);
                break;
            case TwitchUserLeftEvent leftEvent:
                _logger.LogInformation(
                    "User {UserName} left channel {ChannelName}",
                    leftEvent.UserName,
                    leftEvent.ChannelName
                );
                _channelUserStateService.MarkOffline(leftEvent.UserName, leftEvent.ChannelName);
                break;
            default:
                _logger.LogWarning(
                    "Could not parse Twitch event from chat message with event name {ChatMessageEventName}",
                    chatMessage.EventName
                );
                break;
        }
    }
}
