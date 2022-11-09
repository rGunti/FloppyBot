using System.Collections.Immutable;
using AutoMapper;
using FloppyBot.HealthCheck.Receiver;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/health/bots")]
public class HealthController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IHealthCheckReceiver _receiver;

    public HealthController(IHealthCheckReceiver receiver, IMapper mapper)
    {
        _receiver = receiver;
        _mapper = mapper;
    }

    [HttpGet]
    public IReadOnlyDictionary<string, V1HealthCheckData> GetHealthCheck()
    {
        return _receiver.RecordedHealthChecks
            .ToImmutableDictionary(
                d => d.InstanceId,
                d => _mapper.Map<V1HealthCheckData>(d));
    }

    [HttpDelete("{hostName}/{pid}")]
    public IActionResult RestartInstance(
        [FromRoute] string hostName,
        [FromRoute] int pid)
    {
        throw this.NotImplemented();
    }
}
