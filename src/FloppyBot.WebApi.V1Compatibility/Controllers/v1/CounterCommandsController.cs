using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route("api/v1/counter-commands")]
public class CounterCommandsController : ControllerBase
{
    [HttpGet]
    public CounterCommandState[] GetAll()
    {
        throw this.NotImplemented();
    }

    [HttpGet("{messageInterface}/{channel}")]
    public CounterCommandState[] GetAllForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel)
    {
        throw this.NotImplemented();
    }

    [HttpGet("{messageInterface}/{channel}/{command}")]
    public CounterCommandState GetConfigForChannel(
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
        [FromBody] CounterCommandConfig config)
    {
        throw this.NotImplemented();
    }

    [HttpPut("{messageInterface}/{channel}/{command}")]
    public IActionResult UpdateCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] CounterCommandState state)
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

    [HttpDelete("{messageInterface}/{channel}/{command}/state")]
    public IActionResult ResetCommand(
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
