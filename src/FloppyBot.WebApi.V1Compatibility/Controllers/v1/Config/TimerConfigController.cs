using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1.Config;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/config/timer")]
public class TimerConfigController : ControllerBase
{
    [HttpGet]
    public TimerMessageConfig[] GetAll()
    {
        throw this.NotImplemented();
    }

    [HttpPost]
    public IActionResult UpdateAll([FromBody] TimerMessageConfig[] configs)
    {
        throw this.NotImplemented();
    }

    [HttpPost("{messageInterface}/{channel}")]
    public IActionResult UpdateConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] TimerMessageConfig config)
    {
        throw this.NotImplemented();
    }
}
