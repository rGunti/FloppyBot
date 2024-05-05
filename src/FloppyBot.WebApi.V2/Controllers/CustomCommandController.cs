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
    private readonly ICounterStorageService _counterStorageService;

    public CustomCommandController(
        IUserService userService,
        ICustomCommandService customCommandService,
        ICounterStorageService counterStorageService
    )
        : base(userService)
    {
        _customCommandService = customCommandService;
        _counterStorageService = counterStorageService;
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
            .Select(command =>
                CustomCommandDto.FromEntity(command, _counterStorageService.Peek(command.Id))
            )
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

        return CustomCommandDto.FromEntity(command, _counterStorageService.Peek(command.Id));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.EDIT_CUSTOM_COMMANDS)]
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

        if (createDto.Counter is not null)
        {
            var createdCommand =
                _customCommandService.GetCommand(channelId, createDto.Name)
                ?? throw new InvalidOperationException("Command not found after creation.");
            _counterStorageService.Set(createdCommand.Id, createDto.Counter.Value);
        }

        return NoContent();
    }

    [HttpPut("{commandName}")]
    [Authorize(Policy = Permissions.EDIT_CUSTOM_COMMANDS)]
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

        if (updateDto.Counter is not null)
        {
            _counterStorageService.Set(command.Id, updateDto.Counter.Value);
        }

        return NoContent();
    }

    [HttpDelete("{commandName}")]
    [Authorize(Policy = Permissions.EDIT_CUSTOM_COMMANDS)]
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
