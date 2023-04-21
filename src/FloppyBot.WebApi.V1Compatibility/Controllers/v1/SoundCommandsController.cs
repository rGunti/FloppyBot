using AutoMapper;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using FloppyBot.WebApi.V1Compatibility.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.V1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/sound-commands")]
public class SoundCommandsController : ControllerBase
{
    private readonly ICustomCommandService _customCommandService;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public SoundCommandsController(
        IUserService userService,
        ICustomCommandService customCommandService,
        IMapper mapper
    )
    {
        _userService = userService;
        _customCommandService = customCommandService;
        _mapper = mapper;
    }

    [HttpGet]
    public SoundCommand[] GetCommands()
    {
        throw this.UnsupportedFeature("Requesting all commands is not supported");
    }

    private void EnsureChannelAccess(ChannelIdentifier channelIdentifier)
    {
        if (
            !_userService
                .GetAccessibleChannelsForUser(User.GetUserId())
                .Contains(channelIdentifier.ToString())
        )
        {
            throw new NotFoundException(
                $"You don't have access to {channelIdentifier} or it doesn't exist"
            );
        }
    }

    private ChannelIdentifier EnsureChannelAccess(string messageInterface, string channel)
    {
        var channelId = new ChannelIdentifier(messageInterface, channel);
        EnsureChannelAccess(channelId);
        return channelId;
    }

    [HttpGet("{messageInterface}/{channel}")]
    public SoundCommand[] GetCommandsForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        return _customCommandService
            .GetCommandsOfChannel(channelId)
            .Where(V1CompatibilityProfile.IsConvertableForSoundCommand)
            .Select(c => _mapper.Map<SoundCommand>(c))
            .ToArray();
    }

    [HttpGet("{messageInterface}/{channel}/{command}")]
    public SoundCommand GetCommandForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command
    )
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        return _customCommandService
                .GetCommand(channelId, command)
                .Wrap()
                .Where(V1CompatibilityProfile.IsConvertableForSoundCommand)
                .Select(c => _mapper.Map<SoundCommand>(c))
                .FirstOrDefault() ?? throw new NotFoundException($"Command {command} not found");
    }

    [HttpPost("{messageInterface}/{channel}")]
    public IActionResult CreateCommandForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] SoundCommand newCommand
    )
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        newCommand = newCommand with { ChannelId = channelId };

        var convertedCommand = _mapper.Map<CustomCommandDescription>(newCommand);
        if (!_customCommandService.CreateCommand(convertedCommand))
        {
            throw new BadRequestException(
                $"Command with name {newCommand.CommandName} already exists"
            );
        }

        return NoContent();
    }

    [HttpPut("{messageInterface}/{channel}/{command}")]
    public IActionResult UpdateCommandForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command,
        [FromBody] SoundCommand updatedCommand
    )
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        updatedCommand = updatedCommand with { ChannelId = channelId };

        // Check if command is allowed to be updated over legacy API
        NullableObject<CustomCommandDescription> existingCommand = _customCommandService
            .GetCommand(channelId, command)
            .Wrap();
        if (!existingCommand.HasValue)
        {
            throw new NotFoundException($"Command with name {command} does not exist");
        }

        if (
            existingCommand.Select(V1CompatibilityProfile.IsConvertableForSoundCommand).Any(v => !v)
        )
        {
            throw new BadRequestException(
                $"Command {channelId},{command} cannot be updated using the legacy API"
            );
        }

        var convertedCommand = _mapper.Map<CustomCommandDescription>(updatedCommand);
        _customCommandService.UpdateCommand(convertedCommand);
        return NoContent();
    }

    [HttpDelete("{messageInterface}/{channel}/{command}")]
    public IActionResult DeleteCommandFromChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command
    )
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        if (!_customCommandService.DeleteCommand(channelId, command))
        {
            throw new NotFoundException($"Command {command} not found");
        }

        return NoContent();
    }

    [HttpPut("{messageInterface}/{channel}/{command}/rename")]
    public IActionResult RenameCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command,
        [FromBody] string newCommandName
    )
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        NullableObject<CustomCommandDescription> wrappedCommand = _customCommandService
            .GetCommand(channelId, command)
            .Wrap();
        if (!wrappedCommand.HasValue)
        {
            throw new NotFoundException($"Command {command} not found");
        }

        if (_customCommandService.GetCommand(channelId, newCommandName).Wrap().HasValue)
        {
            throw new BadRequestException($"Command with name {newCommandName} already exists");
        }

        CustomCommandDescription customCommand = wrappedCommand.Value with
        {
            Name = newCommandName,
        };
        _customCommandService.UpdateCommand(customCommand);
        return NoContent();
    }
}
