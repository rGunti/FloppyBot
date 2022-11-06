using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route("api/v1/sound-commands")]
public class SoundCommandsController : ControllerBase
{
    [HttpGet]
    public SoundCommand[] GetCommands()
    {
        throw this.NotImplemented();
    }

    [HttpGet("{messageInterface}/{channel}")]
    public SoundCommand GetCommandsForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel)
    {
        throw this.NotImplemented();
    }

    [HttpGet("{messageInterface}/{channel}/{command}")]
    public SoundCommand GetCommandForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command)
    {
        throw this.NotImplemented();
    }

    [HttpPost("{messageInterface}/{channel}")]
    public IActionResult CreateCommandForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] SoundCommand newCommand)
    {
        throw this.NotImplemented();
    }

    [HttpPut("{messageInterface}/{channel}/{command}")]
    public IActionResult UpdateCommandForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command,
        [FromBody] SoundCommand updatedCommand)
    {
        throw this.NotImplemented();
    }

    [HttpDelete("{messageInterface}/{channel}/{command}")]
    public IActionResult DeleteCommandFromChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command)
    {
        throw this.NotImplemented();
    }

    [HttpPut("{messageInterface}/{channel}/{command}/rename")]
    public IActionResult RenameCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command,
        [FromBody] string newCommandName)
    {
        throw this.NotImplemented();
    }
}
