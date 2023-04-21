using FloppyBot.Base.Configuration;
using FloppyBot.Base.Storage.Indexing;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Core.Replier;
using FloppyBot.Commands.Executor.Agent.DistRegistry;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Executor.Agent;

public class ExecutorAgent : BackgroundService
{
    private readonly ICommandExecutor _commandExecutor;

    private readonly DistributedCommandRegistryAdapter _distributedCommandRegistryAdapter;

    private readonly IndexInitializer _indexInitializer;
    private readonly INotificationReceiver<CommandInstruction> _instructionReceiver;
    private readonly ILogger<ExecutorAgent> _logger;

    private readonly IMessageReplier _replier;

    public ExecutorAgent(
        ILogger<ExecutorAgent> logger,
        IConfiguration configuration,
        INotificationReceiverFactory receiverFactory,
        ICommandExecutor commandExecutor,
        IndexInitializer indexInitializer,
        DistributedCommandRegistryAdapter distributedCommandRegistryAdapter,
        IMessageReplier replier
    )
    {
        _logger = logger;
        _instructionReceiver = receiverFactory.GetNewReceiver<CommandInstruction>(
            configuration.GetParsedConnectionString("CommandInput")
        );

        _instructionReceiver.NotificationReceived += OnCommandReceived;

        _commandExecutor = commandExecutor;
        _indexInitializer = indexInitializer;

        _distributedCommandRegistryAdapter = distributedCommandRegistryAdapter;
        _replier = replier;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _indexInitializer.InitializeIndices();

        _logger.LogInformation("Starting Command Executor Agent ...");
        _instructionReceiver.StartListening();
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Command Executor Agent ...");
        _instructionReceiver.StopListening();
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private void OnCommandReceived(CommandInstruction commandInstruction)
    {
#if DEBUG
        _logger.LogDebug("Received command instruction {@CommandInstruction}", commandInstruction);
#endif

        ChatMessage? reply = _commandExecutor.ExecuteCommand(commandInstruction);
        if (reply == null || string.IsNullOrWhiteSpace(reply.Content))
        {
            _logger.LogDebug("Reply was empty (null or whitespace), discarding");
            return;
        }

        _replier.SendMessage(reply);
    }
}
