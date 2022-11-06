using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/commands")]
public class CommandsController : ControllerBase
{
    [HttpGet]
    public CommandInfo[] GetCommands()
    {
        throw this.NotImplemented();
    }

    [HttpGet("config/{messageInterface}/{channel}")]
    public CommandConfigInfo[] GetCommandConfigs(
        [FromRoute] string messageInterface,
        [FromRoute] string channel)
    {
        throw this.NotImplemented();
    }

    [HttpGet("config/{messageInterface}/{channel}/{command}")]
    public CommandConfigInfo GetCommandConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command)
    {
        throw this.NotImplemented();
    }

    [HttpPut("config/{messageInterface}/{channel}/{command}")]
    public IActionResult UpdateCommandConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command,
        [FromBody] CommandConfig commandConfig)
    {
        throw this.NotImplemented();
    }

    [HttpDelete("config/{messageInterface}/{channel}/{command}")]
    public IActionResult DeleteCommandConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command)
    {
        throw this.NotImplemented();
    }
}
