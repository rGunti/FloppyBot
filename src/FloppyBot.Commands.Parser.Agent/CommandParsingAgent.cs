using FloppyBot.Base.Configuration;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Communication;

namespace FloppyBot.Commands.Parser.Agent;

public class CommandParsingAgent : BackgroundService
{
    private readonly INotificationReceiver<ChatMessage> _chatMessageReceiver;
    private readonly INotificationSender _commandMessageSender;
    private readonly ICommandParser _commandParser;
    private readonly ILogger<CommandParsingAgent> _logger;

    public CommandParsingAgent(
        ILogger<CommandParsingAgent> logger,
        IConfiguration configuration,
        INotificationReceiverFactory receiverFactory,
        INotificationSenderFactory senderFactory,
        ICommandParser commandParser
    )
    {
        _logger = logger;
        _commandParser = commandParser;
        _chatMessageReceiver = receiverFactory.GetNewReceiver<ChatMessage>(
            configuration.GetParsedConnectionString("MessageInput")
        );
        _chatMessageReceiver.NotificationReceived += OnNotificationReceived;
        _commandMessageSender = senderFactory.GetNewSender(
            configuration.GetParsedConnectionString("CommandOutput")
        );
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting up Command Parsing Agent ...");
        _logger.LogInformation("Starting receiver ...");
        _chatMessageReceiver.StartListening();

        _logger.LogInformation("Awaiting new messages to receive");
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Command Parsing Agent ...");
        _chatMessageReceiver.StopListening();
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private void OnNotificationReceived(ChatMessage notification)
    {
        if (notification.EventName != SharedEventTypes.CHAT_MESSAGE)
        {
            // Events that aren't chat messages are ignored.
            _logger.LogDebug(
                "Received notification with event name {EventName}, ignoring",
                notification.EventName
            );
            return;
        }

#if DEBUG
        _logger.LogInformation("Received chat message to parse: {@ChatMessage}", notification);
#endif

        CommandInstruction? instruction = _commandParser.ParseCommandFromString(
            notification.Content
        );
        if (instruction != null)
        {
            _logger.LogInformation(
                "Message was parsed to a command successfully, sending to output"
            );
            _commandMessageSender.Send(
                instruction with
                {
                    Context = new CommandContext(notification),
                }
            );
        }
    }
}
