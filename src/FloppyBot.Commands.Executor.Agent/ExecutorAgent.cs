using FloppyBot.Base.Configuration;
using FloppyBot.Base.Storage.Indexing;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Executor.Agent.DistRegistry;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartFormat;

namespace FloppyBot.Commands.Executor.Agent;

public class ExecutorAgent : BackgroundService
{
    private readonly ICommandExecutor _commandExecutor;

    private readonly DistributedCommandRegistryAdapter _distributedCommandRegistryAdapter;

    private readonly IndexInitializer _indexInitializer;
    private readonly INotificationReceiver<CommandInstruction> _instructionReceiver;
    private readonly ILogger<ExecutorAgent> _logger;

    private readonly string _senderConnectionString;
    private readonly INotificationSenderFactory _senderFactory;

    public ExecutorAgent(
        ILogger<ExecutorAgent> logger,
        IConfiguration configuration,
        INotificationReceiverFactory receiverFactory,
        INotificationSenderFactory senderFactory,
        ICommandExecutor commandExecutor,
        IndexInitializer indexInitializer,
        DistributedCommandRegistryAdapter distributedCommandRegistryAdapter)
    {
        _logger = logger;
        _instructionReceiver = receiverFactory.GetNewReceiver<CommandInstruction>(
            configuration.GetParsedConnectionString("CommandInput"));

        _instructionReceiver.NotificationReceived += OnCommandReceived;

        _senderFactory = senderFactory;
        _commandExecutor = commandExecutor;
        _indexInitializer = indexInitializer;
        _senderConnectionString = configuration.GetParsedConnectionString("ResponseOutput");

        _distributedCommandRegistryAdapter = distributedCommandRegistryAdapter;
    }

    private void OnCommandReceived(CommandInstruction commandInstruction)
    {
#if DEBUG
        _logger.LogDebug("Received command instruction {@CommandInstruction}",
            commandInstruction);
#endif

        var reply = _commandExecutor.ExecuteCommand(commandInstruction);

        if (reply == null || string.IsNullOrWhiteSpace(reply.Content))
        {
            _logger.LogDebug("Reply was empty (null or whitespace), discarding");
            return;
        }

        var sender = _senderFactory.GetNewSender(
            _senderConnectionString.FormatSmart(new
            {
                Interface = reply.Identifier.Interface
            }));
        sender.Send(reply);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _indexInitializer.InitializeIndices();
        _distributedCommandRegistryAdapter.Start();

        _logger.LogInformation("Starting Command Executor Agent ...");
        _instructionReceiver.StartListening();
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Command Executor Agent ...");
        _instructionReceiver.StopListening();
        return base.StopAsync(cancellationToken);
    }
}
