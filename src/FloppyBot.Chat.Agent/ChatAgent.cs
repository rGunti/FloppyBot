using FloppyBot.Base.Configuration;
using FloppyBot.Chat.Entities;
using FloppyBot.Communication;

namespace FloppyBot.Chat.Agent;

public class ChatAgent : BackgroundService
{
    private readonly IChatInterface _chatInterface;
    private readonly ILogger<ChatAgent> _logger;
    private readonly INotificationSender _notificationSender;
    private readonly INotificationReceiver<ChatMessage> _replyReceiver;

    public ChatAgent(
        ILogger<ChatAgent> logger,
        IChatInterface chatInterface,
        INotificationSenderFactory senderFactory,
        INotificationReceiverFactory receiverFactory,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _chatInterface = chatInterface;
        _chatInterface.MessageReceived += OnMessageReceived;

        _notificationSender = senderFactory.GetNewSender(
            configuration.GetParsedConnectionString("MessageOutput")
        );
        _replyReceiver = receiverFactory.GetNewReceiver<ChatMessage>(
            configuration.GetParsedConnectionString("MessageInput", true)
        );
        _replyReceiver.NotificationReceived += OnReplyReceived;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting up Chat Agent ...");
        _chatInterface.Connect();
        _replyReceiver.StartListening();
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Chat Agent ...");
        _replyReceiver.StopListening();
        _chatInterface.MessageReceived -= OnMessageReceived;
        _chatInterface.Disconnect();
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private void OnMessageReceived(IChatInterface sourceInterface, ChatMessage chatMessage)
    {
#if DEBUG
        _logger.LogInformation(
            "Received chat message from {SourceInterface}: {ChatMessage}",
            sourceInterface,
            chatMessage
        );
#endif
        _notificationSender.Send(chatMessage);
    }

    private void OnReplyReceived(ChatMessage message)
    {
        _logger.LogTrace("Received reply message {@Message}", message);
        if (!_chatInterface.CanHandleMessageAsResponse(message))
        {
            _logger.LogDebug(
                "Received reply message that cannot be handled by {ChatInterface} (Message Identifier={@MessageIdentifier})",
                _chatInterface.Name,
                message.Identifier
            );
            return;
        }

        _chatInterface.SendMessage(message.Identifier, message.Content);
    }
}
