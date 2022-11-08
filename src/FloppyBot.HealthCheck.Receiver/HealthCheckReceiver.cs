using System.Collections.Concurrent;
using FloppyBot.Communication;
using FloppyBot.HealthCheck.Core;
using FloppyBot.HealthCheck.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FloppyBot.HealthCheck.Receiver;

public class HealthCheckReceiver : IHealthCheckReceiver, IDisposable
{
    private readonly ConcurrentDictionary<string, HealthCheckData> _data;
    private readonly ILogger<HealthCheckReceiver> _logger;
    private readonly INotificationReceiver<HealthCheckData> _receiver;

    public HealthCheckReceiver(
        ILogger<HealthCheckReceiver> logger,
        INotificationReceiverFactory receiverFactory,
        IConfiguration configuration)
    {
        _logger = logger;

        _receiver = receiverFactory.GetNewReceiver<HealthCheckData>(
            configuration.GetHealthCheckConnectionString());
        _receiver.NotificationReceived += ReceiverOnNotificationReceived;
        _receiver.StartListening();

        _data = new ConcurrentDictionary<string, HealthCheckData>();
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing health check receiver ...");
        _receiver.StopListening();
        _data.Clear();
    }

    public IEnumerable<HealthCheckData> RecordedHealthChecks => _data.Values;

    private void ReceiverOnNotificationReceived(HealthCheckData notification)
    {
        _logger.LogDebug("Received health check data ...");
        _data.AddOrUpdate(
            notification.InstanceId,
            _ => notification,
            (_, _) => notification);
    }
}
