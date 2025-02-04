using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Extensions;
using FloppyBot.Commands.Core.Auditing;
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
    private readonly IAuditor _auditor;

    public CommandConfigController(
        IUserService userService,
        ICommandConfigurationService commandConfigurationService,
        IDistributedCommandRegistry distributedCommandRegistry,
        IAuditor auditor
    )
        : base(userService)
    {
        _commandConfigurationService = commandConfigurationService;
        _distributedCommandRegistry = distributedCommandRegistry;
        _auditor = auditor;
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

    [HttpPost("{commandName}")]
    [Authorize(Policy = Permissions.EDIT_CONFIG)]
    public IActionResult SetCommandConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string commandName,
        [FromBody] CommandConfigurationDto commandConfigurationDto
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

        var entity = (commandConfigurationDto with { Id = commandConfiguration.Id }).ToEntity();
        _commandConfigurationService.SetCommandConfiguration(entity);
        _auditor.CommandConfigurationUpdated(User.AsChatUser(), channelId, entity);
        return NoContent();
    }

    [HttpDelete("{commandName}")]
    [Authorize(Policy = Permissions.EDIT_CONFIG)]
    public IActionResult ResetCommand(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string commandName
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        _commandConfigurationService.DeleteCommandConfiguration(channelId, commandName);
        _auditor.CommandConfigurationDeleted(User.AsChatUser(), channelId, commandName);
        return NoContent();
    }

    [HttpPost("{commandName}/disable")]
    [Authorize(Policy = Permissions.EDIT_CONFIG)]
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
        _auditor.CommandConfigurationDisabledSet(
            User.AsChatUser(),
            channelId,
            commandName,
            isDisabled
        );

        return NoContent();
    }
}
