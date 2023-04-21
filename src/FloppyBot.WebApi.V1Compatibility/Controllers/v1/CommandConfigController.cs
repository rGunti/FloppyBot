using AutoMapper;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Config;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using FloppyBot.WebApi.V1Compatibility.Services;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.V1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/commands/config/{messageInterface}/{channel}")]
public class CommandConfigController : ControllerBase
{
    private readonly ICommandConfigurationService _commandConfigurationService;
    private readonly V1CommandConverter _commandConverter;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public CommandConfigController(
        ICommandConfigurationService commandConfigurationService,
        IMapper mapper,
        IUserService userService,
        V1CommandConverter commandConverter
    )
    {
        _commandConfigurationService = commandConfigurationService;
        _mapper = mapper;
        _userService = userService;
        _commandConverter = commandConverter;
    }

    [HttpGet("")]
    public CommandConfigInfo[] GetCommandConfigs(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        Dictionary<string, CommandConfig> availableConfigs = _commandConfigurationService
            .GetCommandConfigurationsForChannel(channelId)
            .Select(c => _mapper.Map<CommandConfig>(c))
            .ToDictionary(c => c.CommandName, c => c);
        return _commandConverter
            .GetAllKnownCommands()
            .Select(
                command =>
                    new CommandConfigInfo(command, availableConfigs.GetValueOrDefault(command.Name))
            )
            .OrderBy(c => c.Info.Name)
            .ToArray();
    }

    [HttpGet("{command}")]
    public CommandConfigInfo GetCommandConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute(Name = "command")] string commandName
    )
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        CommandInfo command = _commandConverter
            .GetCommand(commandName)
            .OrThrow(() => new NotFoundException("Command does not exist"));

        return new CommandConfigInfo(
            command,
            _commandConfigurationService
                .GetCommandConfiguration(channelId, commandName)
                .Select(c => _mapper.Map<CommandConfig>(c))
                .FirstOrDefault()
                ?? new CommandConfig(
                    channelId,
                    command.Name,
                    null,
                    false,
                    false,
                    new CooldownConfig(0, 0, 0)
                )
        );
    }

    [HttpPut("{command}")]
    public IActionResult UpdateCommandConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute(Name = "command")] string commandName,
        [FromBody] CommandConfig commandConfig
    )
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        CommandInfo command = _commandConverter
            .GetCommand(commandName)
            .OrThrow(() => new NotFoundException("Command does not exist"));

        _commandConfigurationService.SetCommandConfiguration(
            _mapper.Map<CommandConfiguration>(
                commandConfig with
                {
                    // Force parameters to prevent URL hacking / abuse
                    ChannelId = channelId,
                    CommandName = commandName,
                }
            )
        );
        return NoContent();
    }

    [HttpDelete("{command}")]
    public IActionResult DeleteCommandConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string command
    )
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        _commandConfigurationService.DeleteCommandConfiguration(channelId, command);
        return NoContent();
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
}
