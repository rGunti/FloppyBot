using FloppyBot.Chat.Entities;

namespace FloppyBot.Chat.Agent;

public class ChatAgent : BackgroundService
{
    private readonly ILogger<ChatAgent> _logger;
    private readonly IChatInterface _chatInterface;

    public ChatAgent(ILogger<ChatAgent> logger, IChatInterface chatInterface)
    {
        _logger = logger;
        _chatInterface = chatInterface;
        _chatInterface.MessageReceived += OnMessageReceived;
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
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Chat Agent ...");
        _chatInterface.MessageReceived -= OnMessageReceived;
        _chatInterface.Disconnect();
        return base.StopAsync(cancellationToken);
    }
}
