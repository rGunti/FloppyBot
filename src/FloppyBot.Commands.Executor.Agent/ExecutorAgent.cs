using System.Collections.Immutable;
using FloppyBot.Base.Configuration;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartFormat;

namespace FloppyBot.Commands.Executor.Agent;

public class ExecutorAgent : BackgroundService
{
    private readonly INotificationReceiver<CommandInstruction> _instructionReceiver;
    private readonly ILogger<ExecutorAgent> _logger;

    private readonly string _senderConnectionString;
    private readonly INotificationSenderFactory _senderFactory;
    private readonly IServiceProvider _serviceProvider;

    public ExecutorAgent(
        ILogger<ExecutorAgent> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        INotificationReceiverFactory receiverFactory,
        INotificationSenderFactory senderFactory)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _instructionReceiver = receiverFactory.GetNewReceiver<CommandInstruction>(
            configuration.GetParsedConnectionString("CommandInput"));

        _instructionReceiver.NotificationReceived += OnCommandReceived;

        _senderFactory = senderFactory;
        _senderConnectionString = configuration.GetParsedConnectionString("ResponseOutput");
    }

    private void OnCommandReceived(CommandInstruction commandInstruction)
    {
#if DEBUG
        _logger.LogDebug("Received command instruction {@CommandInstruction}",
            commandInstruction);
#endif

        _logger.LogTrace("Creating new service scope ...");
        using var scope = _serviceProvider.CreateScope();

        _logger.LogInformation("Trying to find command to execute ...");
        var commands = scope.ServiceProvider
            .GetRequiredService<IEnumerable<IBotCommand>>()
            .Where(c => c.CanExecute(commandInstruction))
            .ToImmutableList();

        IImmutableList<ChatMessage> responses = Enumerable.Empty<ChatMessage>()
            .ToImmutableList();

        if (commands.Any())
        {
            _logger.LogDebug("Executing matching commands ...");
            responses = commands
                .Select(cmd => cmd.Execute(commandInstruction))
                .Where(resp => resp != null)
                .ToImmutableList()!;
        }
        else
        {
            _logger.LogDebug(
                "Could not find a command that can handle {CommandName}",
                commandInstruction.CommandName);
        }

        if (responses.Any())
        {
            _logger.LogInformation(
                "Sending {ResponseCount} messages",
                responses.Count);

            var senderMessageCombination = responses
                .GroupBy(m => m.Identifier.Interface)
                .SelectMany(g =>
                {
                    var sender = _senderFactory.GetNewSender(
                        _senderConnectionString.FormatSmart(new
                        {
                            Interface = g.Key
                        }));
                    return g
                        .Select(m => new
                        {
                            Sender = sender,
                            Message = m
                        });
                });

            foreach (var i in senderMessageCombination)
            {
                i.Sender.Send(i.Message);
            }
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
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
