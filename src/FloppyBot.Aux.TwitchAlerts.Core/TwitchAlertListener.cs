using FloppyBot.Base.Configuration;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Aux.TwitchAlerts.Core;

public class TwitchAlertListener : IDisposable
{
    private static readonly ISet<string> AllowedEvents = new[]
    {
        TwitchEventTypes.SUBSCRIPTION,
        TwitchEventTypes.RE_SUBSCRIPTION,
        TwitchEventTypes.SUBSCRIPTION_GIFT,
        TwitchEventTypes.SUBSCRIPTION_GIFT_COMMUNITY,
    }.ToHashSet();

    private readonly ILogger<TwitchAlertListener> _logger;
    private readonly INotificationReceiver<ChatMessage> _chatMessageReceiver;
    private readonly INotificationSender _responder;

    public TwitchAlertListener(
        ILogger<TwitchAlertListener> logger,
        INotificationReceiverFactory receiverFactor,
        INotificationSenderFactory senderFactory,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _chatMessageReceiver = receiverFactor.GetNewReceiver<ChatMessage>(
            configuration.GetParsedConnectionString("MessageInput")
        );
        _responder = senderFactory.GetNewSender(
            configuration.GetParsedConnectionString("MessageOutput")
        );

        _chatMessageReceiver.NotificationReceived += OnMessageReceived;
    }

    public void Start()
    {
        _logger.LogInformation(
            "Connecting to message input to start listening for incoming messages"
        );
        _chatMessageReceiver.StartListening();
    }

    public void Stop()
    {
        _logger.LogInformation("Shutting down Twitch Alert Listener");
        _chatMessageReceiver.StopListening();
    }

    public void Dispose()
    {
        Stop();
    }

    private void OnMessageReceived(ChatMessage chatMessage)
    {
        if (!AllowedEvents.Contains(chatMessage.EventName))
        {
#if DEBUG
            _logger.LogDebug(
                "Received chat message with event name that is not allowed: {ChatMessageEventName}",
                chatMessage.EventName
            );
#endif
            return;
        }
#if DEBUG
        _logger.LogInformation("Received chat message to count: {@ChatMessage}", chatMessage);
#endif

        if (false)
        {
            // TODO: Send response if required
            _responder.Send(new object());
        }
    }
}
