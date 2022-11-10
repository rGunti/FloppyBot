using FloppyBot.HealthCheck.Core.Entities;

namespace FloppyBot.HealthCheck.Receiver;

public interface IHealthCheckReceiver
{
    IEnumerable<HealthCheckData> RecordedHealthChecks { get; }
}
