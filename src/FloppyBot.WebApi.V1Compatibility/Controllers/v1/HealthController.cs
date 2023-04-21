using System.Collections.Immutable;
using AutoMapper;
using FloppyBot.HealthCheck.KillSwitch;
using FloppyBot.HealthCheck.Receiver;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.V1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/health/bots")]
[Authorize(Policy = Permissions.READ_BOT)]
public class HealthController : ControllerBase
{
    private readonly IKillSwitchTrigger _killSwitchTrigger;
    private readonly IMapper _mapper;
    private readonly IHealthCheckReceiver _receiver;

    public HealthController(
        IHealthCheckReceiver receiver,
        IMapper mapper,
        IKillSwitchTrigger killSwitchTrigger
    )
    {
        _receiver = receiver;
        _mapper = mapper;
        _killSwitchTrigger = killSwitchTrigger;
    }

    [HttpGet]
    public IReadOnlyDictionary<string, V1HealthCheckData> GetHealthCheck()
    {
        return _receiver.RecordedHealthChecks.ToImmutableDictionary(
            d => d.InstanceId,
            d => _mapper.Map<V1HealthCheckData>(d)
        );
    }

    [HttpDelete("{hostName}/{pid}")]
    [Authorize(Permissions.RESTART_BOT)]
    public IActionResult RestartInstance([FromRoute] string hostName, [FromRoute] int pid)
    {
        var instanceId = _receiver.RecordedHealthChecks
            .Where(d => d.HostName == hostName && d.Process.Pid == pid)
            .Select(d => d.InstanceId)
            .FirstOrDefault();
        if (instanceId == null)
        {
            throw new NotFoundException($"Instance does not exist");
        }

        _killSwitchTrigger.RequestRestart(instanceId);
        return Accepted();
    }
}
