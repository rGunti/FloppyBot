using FloppyBot.HealthCheck.Core.Entities;

namespace FloppyBot.HealthCheck.Core;

public interface IHealthCheckProducer
{
    HealthCheckData GetHealthCheckData();
}
