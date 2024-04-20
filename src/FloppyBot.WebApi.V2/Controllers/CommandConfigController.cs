using FloppyBot.Base.Extensions;
using FloppyBot.Commands.Core.Config;
using FloppyBot.Commands.Registry;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V2.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers;

[ApiController]
[Route("api/v2/commands/config/{messageInterface}/{channel}")]
[Authorize(Policy = Permissions.READ_CONFIG)]
public class CommandConfigController : ChannelScopedController
{
    private readonly ICommandConfigurationService _commandConfigurationService;
    private readonly IDistributedCommandRegistry _distributedCommandRegistry;

    public CommandConfigController(
        IUserService userService,
        ICommandConfigurationService commandConfigurationService,
        IDistributedCommandRegistry distributedCommandRegistry
    )
        : base(userService)
    {
        _commandConfigurationService = commandConfigurationService;
        _distributedCommandRegistry = distributedCommandRegistry;
    }

    [HttpGet]
    public CommandReportDto[] GetCommandConfigs(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        var availableConfigs = _commandConfigurationService
            .GetCommandConfigurationsForChannel(channelId)
            .Select(CommandConfigurationDto.FromEntity)
            .ToDictionary(c => c.CommandName, c => c);
        var commands = _distributedCommandRegistry
            .GetAllCommands()
            .Select(CommandAbstractDto.FromEntity)
            .Select(cmd => new CommandReportDto(cmd, availableConfigs.GetValueOrDefault(cmd.Name)))
            .OrderBy(c => c.Command.Name)
            .ToArray();

        return commands;
    }

    [HttpGet("{commandName}")]
    public CommandReportDto GetCommandReport(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string commandName
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        var command = _distributedCommandRegistry
            .GetCommand(commandName)
            .OrThrow(() => new NotFoundException("Command does not exist"));
        var commandConfiguration =
            _commandConfigurationService
                .GetCommandConfiguration(channelId, commandName)
                .FirstOrDefault()
            ?? new CommandConfiguration
            {
                CommandName = command.Name,
                ChannelId = channelId,
                Disabled = false,
            };

        return new CommandReportDto(
            CommandAbstractDto.FromEntity(command),
            CommandConfigurationDto.FromEntity(commandConfiguration)
        );
    }

    [HttpPost("{commandName}/disable")]
    public IActionResult DisableCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string commandName,
        [FromQuery] bool isDisabled
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        var command = _distributedCommandRegistry
            .GetCommand(commandName)
            .OrThrow(() => new NotFoundException("Command does not exist"));
        var commandConfiguration =
            _commandConfigurationService
                .GetCommandConfiguration(channelId, commandName)
                .FirstOrDefault()
            ?? new CommandConfiguration
            {
                CommandName = command.Name,
                ChannelId = channelId,
                Disabled = false,
            };

        _commandConfigurationService.SetCommandConfiguration(
            commandConfiguration with
            {
                Disabled = isDisabled,
            }
        );

        return NoContent();
    }
}
