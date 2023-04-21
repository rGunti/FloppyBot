using FloppyBot.Base.Configuration;
using FloppyBot.Chat.Entities;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Aux.MessageCounter.Core;

public class MessageCounter : IDisposable
{
    private readonly INotificationReceiver<ChatMessage> _chatMessageReceiver;
    private readonly ILogger<MessageCounter> _logger;
    private readonly IMessageOccurrenceService _messageOccurrenceService;

    public MessageCounter(
        INotificationReceiverFactory receiverFactory,
        IConfiguration configuration,
        IMessageOccurrenceService messageOccurrenceService,
        ILogger<MessageCounter> logger
    )
    {
        _messageOccurrenceService = messageOccurrenceService;
        _logger = logger;
        _chatMessageReceiver = receiverFactory.GetNewReceiver<ChatMessage>(
            configuration.GetParsedConnectionString("MessageInput")
        );
        _chatMessageReceiver.NotificationReceived += OnMessageReceived;
    }

    public void Dispose()
    {
        Stop();
    }

    private void OnMessageReceived(ChatMessage chatMessage)
    {
#if DEBUG
        _logger.LogInformation("Received chat message to count: {@ChatMessage}", chatMessage);
#endif
        _messageOccurrenceService.StoreMessage(chatMessage);
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
        _logger.LogInformation("Shutting down Message Counter");
        _chatMessageReceiver.StopListening();
    }
}
