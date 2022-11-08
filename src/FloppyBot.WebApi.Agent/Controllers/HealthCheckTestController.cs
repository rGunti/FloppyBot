using FloppyBot.HealthCheck.Core.Entities;
using FloppyBot.HealthCheck.Receiver;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.Agent.Controllers;

[ApiController]
[Route("api/v2/bots/health")]
public class HealthCheckTestController : ControllerBase
{
    private readonly IHealthCheckReceiver _healthCheckReceiver;

    public HealthCheckTestController(IHealthCheckReceiver healthCheckReceiver)
    {
        _healthCheckReceiver = healthCheckReceiver;
    }

    [HttpGet]
    public HealthCheckData[] GetHealthChecks()
    {
        return _healthCheckReceiver.RecordedHealthChecks
            .OrderBy(h => h.App.Service)
            .ThenBy(h => h.App.InstanceName)
            .ThenBy(h => h.InstanceId)
            .ToArray();
    }
}
