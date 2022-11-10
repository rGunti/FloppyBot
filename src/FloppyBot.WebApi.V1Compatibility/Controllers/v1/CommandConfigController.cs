using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/commands/config/{messageInterface}/{channel}")]
public class CommandConfigController : ControllerBase
{
    [HttpGet("")]
    public CommandConfigInfo[] GetCommandConfigs(
        [FromRoute] string messageInterface,
        [FromRoute] string channel)
    {
        throw this.NotImplemented();
    }

    [HttpGet("{command}")]
    public CommandConfigInfo GetCommandConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command)
    {
        throw this.NotImplemented();
    }

    [HttpPut("{command}")]
    public IActionResult UpdateCommandConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command,
        [FromBody] CommandConfig commandConfig)
    {
        throw this.NotImplemented();
    }

    [HttpDelete("{command}")]
    public IActionResult DeleteCommandConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command)
    {
        throw this.NotImplemented();
    }
}
