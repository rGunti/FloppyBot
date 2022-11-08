using System.Diagnostics;
using FloppyBot.Base.Clock;
using FloppyBot.HealthCheck.Core.Entities;
using FloppyBot.Version;
using Microsoft.Extensions.Configuration;

namespace FloppyBot.HealthCheck.Core;

public class HealthCheckProducer : IHealthCheckProducer
{
    private readonly string _instanceName;
    private readonly ITimeProvider _timeProvider;

    public HealthCheckProducer(
        ITimeProvider timeProvider,
        IConfiguration configuration)
    {
        _timeProvider = timeProvider;
        _instanceName = configuration["InstanceName"] ?? "Unnamed Instance";
    }

    public HealthCheckData GetHealthCheckData()
    {
        var info = AboutThisApp.Info;
        var process = Process.GetCurrentProcess();
        return new HealthCheckData(
            _timeProvider.GetCurrentUtcTime(),
            Environment.MachineName,
            new AppInfo(
                info.ServiceName,
                info.Version,
                _instanceName),
            new ProcessInfo(
                process.Id,
                process.WorkingSet64,
                process.StartTime));
    }
}
