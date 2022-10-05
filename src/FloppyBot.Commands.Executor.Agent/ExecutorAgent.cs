using System.Collections.Immutable;
using FloppyBot.Base.Configuration;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Executor.Agent;

public class ExecutorAgent : BackgroundService
{
    private readonly INotificationReceiver<CommandInstruction> _instructionReceiver;
    private readonly ILogger<ExecutorAgent> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ExecutorAgent(
        ILogger<ExecutorAgent> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        INotificationReceiverFactory receiverFactory)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _instructionReceiver = receiverFactory.GetNewReceiver<CommandInstruction>(
            configuration.GetParsedConnectionString("CommandInput"));

        _instructionReceiver.NotificationReceived += OmCommandReceived;
    }

    private void OmCommandReceived(CommandInstruction commandInstruction)
    {
#if DEBUG
        _logger.LogDebug("Received command instruction {@CommandInstruction}",
            commandInstruction);
#endif

        _logger.LogTrace("Creating new service scope ...");
        using (var scope = _serviceProvider.CreateScope())
        {
            _logger.LogInformation("Trying to find command to execute ...");
            var commands = scope.ServiceProvider
                .GetRequiredService<IEnumerable<IBotCommand>>()
                .Where(c => c.CanExecute(commandInstruction))
                .ToImmutableList();

            if (commands.Any())
            {
                commands.ForEach(cmd =>
                {
                    // TODO: Handle return value
                    cmd.Execute(commandInstruction);
                });
            }
            else
            {
                _logger.LogDebug(
                    "Could not find a command that can handle {CommandName}",
                    commandInstruction.CommandName);
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
