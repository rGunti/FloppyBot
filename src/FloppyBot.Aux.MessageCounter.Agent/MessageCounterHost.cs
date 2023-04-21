using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Aux.MessageCounter.Agent;

public class MessageCounterHost : BackgroundService
{
    private readonly ILogger<MessageCounterHost> _logger;
    private readonly Core.MessageCounter _messageCounter;

    public MessageCounterHost(
        ILogger<MessageCounterHost> logger,
        Core.MessageCounter messageCounter
    )
    {
        _logger = logger;
        _messageCounter = messageCounter;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting up Message Counter Agent ...");
        _messageCounter.Start();

        _logger.LogInformation("Awaiting new messages to count");
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Message Counter Agent ...");
        _messageCounter.Stop();
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
