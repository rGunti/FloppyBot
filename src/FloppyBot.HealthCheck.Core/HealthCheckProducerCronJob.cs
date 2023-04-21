using FloppyBot.Base.Cron;
using FloppyBot.Base.Cron.Attributes;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FloppyBot.HealthCheck.Core;

[CronInterval(Milliseconds = 5000)]
public class HealthCheckProducerCronJob : ICronJob
{
    private readonly IHealthCheckProducer _healthCheckProducer;
    private readonly ILogger<HealthCheckProducerCronJob> _logger;
    private readonly INotificationSender _notificationSender;

    public HealthCheckProducerCronJob(
        ILogger<HealthCheckProducerCronJob> logger,
        IHealthCheckProducer healthCheckProducer,
        INotificationSenderFactory notificationSenderFactory,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _healthCheckProducer = healthCheckProducer;
        _notificationSender = notificationSenderFactory.GetNewSender(
            configuration.GetHealthCheckConnectionString()
        );
    }

    public void Run()
    {
        var healthCheckData = _healthCheckProducer.GetHealthCheckData();
        _logger.LogTrace("Collected Health Check data: {@HealthCheckData}", healthCheckData);
        _notificationSender.Send(healthCheckData);
    }
}
