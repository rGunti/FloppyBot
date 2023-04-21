using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.V1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/counter-commands")]
public class CounterCommandsController : ControllerBase
{
    private const string ERROR_UNSUPPORTED = "Counter Commands are not supported in V2.";

    [HttpGet]
    public CounterCommandState[] GetAll()
    {
        throw this.UnsupportedFeature(ERROR_UNSUPPORTED);
    }

    [HttpGet("{messageInterface}/{channel}")]
    public CounterCommandState[] GetAllForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        throw this.UnsupportedFeature(ERROR_UNSUPPORTED);
    }

    [HttpGet("{messageInterface}/{channel}/{command}")]
    public CounterCommandState GetConfigForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command
    )
    {
        throw this.UnsupportedFeature(ERROR_UNSUPPORTED);
    }

    [HttpPost("{messageInterface}/{channel}")]
    public IActionResult CreateCommandForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] CounterCommandConfig config
    )
    {
        throw this.UnsupportedFeature(ERROR_UNSUPPORTED);
    }

    [HttpPut("{messageInterface}/{channel}/{command}")]
    public IActionResult UpdateCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] CounterCommandState state
    )
    {
        throw this.UnsupportedFeature(ERROR_UNSUPPORTED);
    }

    [HttpDelete("{messageInterface}/{channel}/{command}")]
    public IActionResult DeleteCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command
    )
    {
        throw this.UnsupportedFeature(ERROR_UNSUPPORTED);
    }

    [HttpDelete("{messageInterface}/{channel}/{command}/state")]
    public IActionResult ResetCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command
    )
    {
        throw this.UnsupportedFeature(ERROR_UNSUPPORTED);
    }

    [HttpPut("{messageInterface}/{channel}/{command}/rename")]
    public IActionResult RenameCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command,
        [FromBody] string newCommandName
    )
    {
        throw this.UnsupportedFeature(ERROR_UNSUPPORTED);
    }
}
