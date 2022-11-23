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

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/custom-commands")]
public class CustomCommandsController : ControllerBase
{
    private readonly ICustomCommandService _customCommandService;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public CustomCommandsController(
        ICustomCommandService customCommandService,
        IUserService userService,
        IMapper mapper)
    {
        _customCommandService = customCommandService;
        _userService = userService;
        _mapper = mapper;
    }

    private void EnsureChannelAccess(ChannelIdentifier channelIdentifier)
    {
        if (!_userService.GetAccessibleChannelsForUser(User.GetUserId())
                .Contains(channelIdentifier.ToString()))
        {
            throw new NotFoundException($"You don't have access to {channelIdentifier} or it doesn't exist");
        }
    }

    private ChannelIdentifier EnsureChannelAccess(string messageInterface, string channel)
    {
        var channelId = new ChannelIdentifier(messageInterface, channel);
        EnsureChannelAccess(channelId);
        return channelId;
    }

    [HttpGet]
    public CustomCommand[] GetAll()
    {
        throw this.NotImplemented();
    }

    [HttpGet("{messageInterface}/{channel}")]
    public CustomCommand[] GetCommandsForChannel(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel)
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        return _customCommandService
            .GetCommandsOfChannel(channelId)
            .Where(V1CompatibilityProfile.IsConvertableForTextCommand)
            .Select(c => _mapper.Map<CustomCommand>(c))
            .ToArray();
    }

    [HttpGet("{messageInterface}/{channel}/{command}")]
    public CustomCommand GetCommand(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel,
        [FromRoute]
        string command)
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        return _customCommandService
            .GetCommand(channelId, command)
            .Wrap()
            .Select(c => _mapper.Map<CustomCommand>(c))
            .FirstOrDefault() ?? throw new NotFoundException($"Command {command} not found");
    }

    [HttpGet("{messageInterface}/{channel}/{command}/counter")]
    public int GetCommandCounter(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel,
        [FromRoute]
        string command)
    {
        throw this.NotImplemented();
    }

    [HttpPut("{messageInterface}/{channel}/{command}/counter")]
    public IActionResult UpdateCommandCounter(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel,
        [FromRoute]
        string command,
        [FromBody]
        int counterValue)
    {
        throw this.NotImplemented();
    }

    [HttpPut("{messageInterface}/{channel}/{command}/rename")]
    public IActionResult RenameCommand(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel,
        [FromRoute]
        string command,
        [FromBody]
        string newCommandName)
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
            Name = newCommandName
        };
        _customCommandService.UpdateCommand(customCommand);
        return NoContent();
    }

    [HttpPost("{messageInterface}/{channel}")]
    public IActionResult CreateCommand(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel,
        [FromBody]
        CustomCommand newCommand)
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        if (string.IsNullOrWhiteSpace(newCommand.Channel))
        {
            newCommand = newCommand with
            {
                Channel = channelId
            };
        }

        var convertedCommand = _mapper.Map<CustomCommandDescription>(newCommand);
        if (!_customCommandService.CreateCommand(convertedCommand))
        {
            throw new BadRequestException($"Command with name {newCommand.Command} already exists");
        }

        return NoContent();
    }

    [HttpPut("{messageInterface}/{channel}/{command}")]
    public IActionResult UpdateCommand(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel,
        [FromRoute]
        string command,
        [FromBody]
        CustomCommand updatedCommand)
    {
        throw this.NotImplemented();
    }

    [HttpDelete("{messageInterface}/{channel}/{command}")]
    public IActionResult DeleteCommand(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel,
        [FromRoute]
        string command)
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        if (!_customCommandService.DeleteCommand(channelId, command))
        {
            throw new NotFoundException($"Command {command} not found");
        }

        return NoContent();
    }
}
