using FloppyBot.Communication;
using FloppyBot.Version;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloppyBot.HealthCheck.KillSwitch;

public class KillSwitchReceiver : IDisposable
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<KillSwitchReceiver> _logger;
    private readonly INotificationReceiver<KillSwitchMessage> _receiver;

    public KillSwitchReceiver(
        ILogger<KillSwitchReceiver> logger,
        INotificationReceiverFactory receiverFactory,
        IConfiguration configuration,
        IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _receiver = receiverFactory.GetNewReceiver<KillSwitchMessage>(
            configuration.GetKillSwitchConnectionString());
        _receiver.NotificationReceived += OnKillSwitchMessageReceived;

        _logger.LogInformation("Start listening for Kill Switch messages");
        _receiver.StartListening();
    }

    public void Dispose()
    {
        _logger.LogInformation($"Stopping and disposing of {nameof(KillSwitchReceiver)}");
        _receiver.StopListening();
    }

    private void OnKillSwitchMessageReceived(KillSwitchMessage notification)
    {
        if (AboutThisApp.Info.InstanceId != notification.InstanceId)
        {
            return;
        }

        _logger.LogWarning("Will terminate this instance in 2.5 seconds");
        Task.Delay(2500).ContinueWith(_ =>
        {
            _logger.LogWarning("Invoking shutdown ...");
            _appLifetime.StopApplication();
        });

        _logger.LogInformation("Stopping receiver ...");
        _receiver.StopListening();
    }
}
