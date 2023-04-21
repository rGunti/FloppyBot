using FloppyBot.Base.Configuration;
using FloppyBot.Chat.Entities;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Chat.Console.Agent;

internal class ConsoleChatAgent : BackgroundService
{
    private readonly ConsoleChatInterface _chatInterface;
    private readonly ILogger<ConsoleChatAgent> _logger;
    private readonly INotificationReceiver<ChatMessage> _receiver;
    private readonly INotificationSender _sender;

    public ConsoleChatAgent(
        ILogger<ConsoleChatAgent> logger,
        INotificationSenderFactory senderFactory,
        INotificationReceiverFactory receiverFactory,
        IConfiguration configuration,
        ConsoleChatInterface chatInterface
    )
    {
        _logger = logger;
        _chatInterface = chatInterface;
        _chatInterface.MessageReceived += OnMessageReceived;

        _sender = senderFactory.GetNewSender(
            configuration.GetParsedConnectionString("MessageOutput")
        );
        _receiver = receiverFactory.GetNewReceiver<ChatMessage>(
            configuration.GetParsedConnectionString("MessageInput")
        );
        _receiver.NotificationReceived += OnMessageReceived;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Welcome to the FloppyBot Console Agent! "
                + "Please note that this is a development tool and not meant to be run in production environments!"
        );
        _logger.LogInformation("Starting Console Agent ...");
        _chatInterface.Connect();
        _receiver.StartListening();
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Console Agent (you might need to press [ENTER]) ...");
        _chatInterface.Disconnect();
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(500, stoppingToken);
        }
    }

    private void OnMessageReceived(ChatMessage notification)
    {
        if (notification.Identifier.Interface == _chatInterface.Name)
        {
            _chatInterface.SendMessage(notification.Content);
        }
    }

    private void OnMessageReceived(IChatInterface sourceInterface, ChatMessage chatMessage)
    {
        _logger.LogDebug("Sending message {@Message}", chatMessage);
        _sender.Send(chatMessage);
    }
}
