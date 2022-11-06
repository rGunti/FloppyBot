using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route("api/v1/custom-commands")]
public class CustomCommandsController : ControllerBase
{
    [HttpGet]
    public CustomCommand[] GetAll()
    {
        throw this.NotImplemented();
    }

    [HttpGet("{messageInterface}/{channel}")]
    public CustomCommand[] GetCommandsForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel)
    {
        throw this.NotImplemented();
    }

    [HttpGet("{messageInterface}/{channel}/{command}")]
    public CustomCommand GetCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command)
    {
        throw this.NotImplemented();
    }

    [HttpGet("{messageInterface}/{channel}/{command}/counter")]
    public int GetCommandCounter(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command)
    {
        throw this.NotImplemented();
    }

    [HttpPut("{messageInterface}/{channel}/{command}/counter")]
    public IActionResult UpdateCommandCounter(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command,
        [FromBody] int counterValue)
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

    [HttpPost("{messageInterface}/{channel}")]
    public IActionResult CreateCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] CustomCommand newCommand)
    {
        throw this.NotImplemented();
    }

    [HttpPut("{messageInterface}/{channel}/{command}")]
    public IActionResult UpdateCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command,
        [FromBody] CustomCommand updatedCommand)
    {
        throw this.NotImplemented();
    }

    [HttpDelete("{messageInterface}/{channel}/{command}")]
    public IActionResult DeleteCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command)
    {
        throw this.NotImplemented();
    }
}
