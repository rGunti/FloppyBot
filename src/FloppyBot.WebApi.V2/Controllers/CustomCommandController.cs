using FloppyBot.Commands.Custom.Storage;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.V2.Dtos.CustomCommands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers;

[ApiController]
[Route("api/v2/custom-commands/{messageInterface}/{channel}")]
[Route("api/v2/commands/custom/{messageInterface}/{channel}")]
[Authorize(Policy = Permissions.READ_CUSTOM_COMMANDS)]
public class CustomCommandController : ChannelScopedController
{
    private readonly ICustomCommandService _customCommandService;

    public CustomCommandController(
        IUserService userService,
        ICustomCommandService customCommandService
    )
        : base(userService)
    {
        _customCommandService = customCommandService;
    }

    [HttpGet]
    public List<CustomCommandDto> GetCommands(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        return _customCommandService
            .GetCommandsOfChannel(channelId)
            .Select(CustomCommandDto.FromEntity)
            .ToList();
    }

    [HttpGet("{commandName}")]
    public ActionResult<CustomCommandDto> GetCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string commandName
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        var command = _customCommandService.GetCommand(channelId, commandName);
        if (command is null)
        {
            return NotFound();
        }

        return CustomCommandDto.FromEntity(command);
    }

    [HttpPost]
    public IActionResult CreateCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] CustomCommandDto createDto
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        var command = createDto.ToEntity() with { Owners = [channelId] };
        if (!_customCommandService.CreateCommand(command))
        {
            return BadRequest();
        }

        return NoContent();
    }

    [HttpPut("{commandName}")]
    public IActionResult UpdateCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string commandName,
        [FromBody] CustomCommandDto updateDto
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        var command = _customCommandService.GetCommand(channelId, commandName);
        if (command is null)
        {
            return NotFound();
        }

        command = updateDto.ToEntity().WithId(command.Id) with { Owners = command.Owners, };
        _customCommandService.UpdateCommand(command);

        return NoContent();
    }

    [HttpDelete("{commandName}")]
    public IActionResult DeleteCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string commandName
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        if (!_customCommandService.DeleteCommand(channelId, commandName))
        {
            return NotFound();
        }

        return NoContent();
    }
}
