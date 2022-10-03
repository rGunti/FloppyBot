using FloppyBot.Base.Configuration;
using FloppyBot.Chat.Entities;
using FloppyBot.Communication;

namespace FloppyBot.Chat.Agent;

public class ChatAgent : BackgroundService
{
    private readonly ILogger<ChatAgent> _logger;
    private readonly IChatInterface _chatInterface;
    private readonly INotificationSender _notificationSender;

    public ChatAgent(
        ILogger<ChatAgent> logger,
        IChatInterface chatInterface,
        INotificationSenderFactory senderFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _chatInterface = chatInterface;
        _chatInterface.MessageReceived += OnMessageReceived;

        _notificationSender = senderFactory.GetNewSender(
            configuration.GetParsedConnectionString("MessageOutput"));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting up Chat Agent ...");
        _chatInterface.Connect();
        return Task.CompletedTask;
    }

    private void OnMessageReceived(IChatInterface sourceInterface, ChatMessage chatMessage)
    {
        #if DEBUG
        _logger.LogInformation("Received chat message from {SourceInterface}: {ChatMessage}",
            sourceInterface,
            chatMessage);
        #endif
        _notificationSender.Send(chatMessage);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Chat Agent ...");
        _chatInterface.MessageReceived -= OnMessageReceived;
        _chatInterface.Disconnect();
        return base.StopAsync(cancellationToken);
    }
}
