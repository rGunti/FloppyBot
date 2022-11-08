using FloppyBot.Base.Cron;
using FloppyBot.Base.Cron.Attributes;
using Microsoft.Extensions.Logging;

namespace FloppyBot.HealthCheck.Core;

[CronInterval(Milliseconds = 5000)]
public class HealthCheckProducerCronJob : ICronJob
{
    private readonly IHealthCheckProducer _healthCheckProducer;
    private readonly ILogger<HealthCheckProducerCronJob> _logger;

    public HealthCheckProducerCronJob(
        ILogger<HealthCheckProducerCronJob> logger,
        IHealthCheckProducer healthCheckProducer)
    {
        _logger = logger;
        _healthCheckProducer = healthCheckProducer;
    }

    public void Run()
    {
        var healthCheckData = _healthCheckProducer.GetHealthCheckData();
        _logger.LogInformation(
            "Collected Health Check data: {@HealthCheckData}",
            healthCheckData);
    }
}
