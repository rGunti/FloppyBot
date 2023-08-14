using FloppyBot.Aux.TwitchAlerts.Core;

namespace FloppyBot.Aux.TwitchAlerts.Agent;

public class TwitchAlertHost : BackgroundService
{
    private readonly ILogger<TwitchAlertHost> _logger;
    private readonly TwitchAlertListener _listener;

    public TwitchAlertHost(ILogger<TwitchAlertHost> logger, TwitchAlertListener listener)
    {
        _logger = logger;
        _listener = listener;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting up Twitch Alert Agent ...");
        _listener.Start();

        _logger.LogInformation("Awaiting new messages to count");
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Twitch Alert Agent ...");
        _listener.Stop();
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
