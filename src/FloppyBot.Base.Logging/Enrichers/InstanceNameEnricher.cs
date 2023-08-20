using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace FloppyBot.Base.Logging.Enrichers;

public class InstanceNameEnricher : ILogEventEnricher
{
    private readonly string _instanceName;

    public InstanceNameEnricher(IConfiguration config)
    {
        _instanceName = config["HealthCheck"];
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var instanceNameProperty = propertyFactory.CreateProperty(
            "FloppyBotInstanceName",
            _instanceName
        );
        logEvent.AddOrUpdateProperty(instanceNameProperty);
    }
}
