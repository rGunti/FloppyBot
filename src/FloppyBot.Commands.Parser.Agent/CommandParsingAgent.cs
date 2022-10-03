using FloppyBot.Base.Configuration;
using FloppyBot.Chat.Entities;
using FloppyBot.Communication;

namespace FloppyBot.Commands.Parser.Agent;

public class CommandParsingAgent : BackgroundService
{
    private readonly ILogger<CommandParsingAgent> _logger;
    private readonly INotificationReceiver<ChatMessage> _chatMessageReceiver;
    private readonly INotificationSender _commandMessageSender;
    private readonly ICommandParser _commandParser;

    public CommandParsingAgent(
        ILogger<CommandParsingAgent> logger,
        IConfiguration configuration,
        INotificationReceiverFactory receiverFactory,
        INotificationSenderFactory senderFactory,
        ICommandParser commandParser)
    {
        _logger = logger;
        _commandParser = commandParser;
        _chatMessageReceiver = receiverFactory.GetNewReceiver<ChatMessage>(
            configuration.GetParsedConnectionString("MessageInput"));
        _chatMessageReceiver.NotificationReceived += OnNotificationReceived;
        _commandMessageSender = senderFactory.GetNewSender(
            configuration.GetParsedConnectionString("CommandOutput"));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting up Command Parsing Agent ...");
        _logger.LogInformation("Starting receiver ...");
        _chatMessageReceiver.StartListening();
        
        _logger.LogInformation("Awaiting new messages to receive");
        return Task.CompletedTask;
    }

    private void OnNotificationReceived(ChatMessage notification)
    {
        #if DEBUG
        _logger.LogInformation(
            "Received chat message to parse: {@ChatMessage}",
            notification);
        #endif

        CommandInstruction? instruction = _commandParser.ParseCommandFromString(notification.Content);
        if (instruction != null)
        {
            _logger.LogInformation("Message was parsed to a command successfully, sending to output");
            _commandMessageSender.Send(instruction);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Command Parsing Agent ...");
        _chatMessageReceiver.StopListening();
        return base.StopAsync(cancellationToken);
    }
}
