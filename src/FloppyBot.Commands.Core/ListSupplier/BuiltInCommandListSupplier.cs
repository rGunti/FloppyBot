using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.ListSupplier;

public class BuiltInCommandListSupplier : ICommandListSupplier
{
    private readonly ConcurrentDictionary<CommandInfo, PrivilegeLevel> _cachedPrivilegeLevels;
    private readonly ConcurrentDictionary<
        CommandInfo,
        IImmutableSet<string>
    > _cachedSourceInterfaces;
    private readonly ImmutableHashSet<CommandInfo> _commands;
    private readonly ILogger<BuiltInCommandListSupplier> _logger;

    public BuiltInCommandListSupplier(
        ILogger<BuiltInCommandListSupplier> logger,
        ICommandExecutor commandExecutor
    )
    {
        _logger = logger;

        _cachedSourceInterfaces = new ConcurrentDictionary<CommandInfo, IImmutableSet<string>>();
        _cachedPrivilegeLevels = new ConcurrentDictionary<CommandInfo, PrivilegeLevel>();

        _commands = commandExecutor.KnownCommands.ToImmutableHashSet();
        PrecalculateCommands();
    }

    public IEnumerable<string> GetCommandList(CommandInstruction commandInstruction)
    {
        ChannelIdentifier channelId =
            commandInstruction.Context!.SourceMessage.Identifier.GetChannel();
        PrivilegeLevel privilegeLevel = commandInstruction
            .Context!
            .SourceMessage
            .Author
            .PrivilegeLevel;

        return GetCommandList(channelId, privilegeLevel).Select(c => c.PrimaryCommandName);
    }

    private void PrecalculateCommands()
    {
        _logger.LogDebug("Pre-calculating commands ...");
        foreach (CommandInfo commandInfo in _commands)
        {
            _cachedSourceInterfaces.GetOrAdd(commandInfo, GetInterfacesForCommand);
            _cachedPrivilegeLevels.GetOrAdd(commandInfo, GetPrivilegeLevelFromCommand);
        }
    }

    private IEnumerable<CommandInfo> GetCommandList(
        ChannelIdentifier channelIdentifier,
        PrivilegeLevel privilegeLevel
    )
    {
        return _commands
            .Where(c => !c.IsVariable)
            .Where(c => CanBeAccessedOnInterface(c, channelIdentifier))
            .Where(c => CanBeAccessedWithPrivilegeLevel(c, privilegeLevel));
    }

    private bool CanBeAccessedOnInterface(
        CommandInfo commandInfo,
        ChannelIdentifier channelIdentifier
    )
    {
        _logger.LogTrace(
            "Fetching allowed interfaces for {CommandName}",
            commandInfo.PrimaryCommandName
        );
        IImmutableSet<string> commands = _cachedSourceInterfaces.GetOrAdd(
            commandInfo,
            GetInterfacesForCommand
        );
        _logger.LogTrace(
            "Comparing command {CommandName} interface whitelist {CommandInterfaceWhiteList} against source channel {SourceChannel}",
            commandInfo.PrimaryCommandName,
            commands.ToArray(),
            channelIdentifier.Interface
        );
        return commands.Count == 0 || commands.Contains(channelIdentifier.Interface);
    }

    private IImmutableSet<string> GetInterfacesForCommand(CommandInfo commandInfo)
    {
        _logger.LogDebug("Calculating allowed interfaces for command {Command}", commandInfo);
        IEnumerable<string> handlerInterfaces = commandInfo
            .HandlerMethod.GetCustomAttributes<SourceInterfaceGuardAttribute>()
            .SelectMany(a => a.AllowedMessageInterfaces);
        IEnumerable<string> hostInterfaces = commandInfo
            .ImplementingType.GetCustomAttributes<SourceInterfaceGuardAttribute>()
            .SelectMany(a => a.AllowedMessageInterfaces);

        ImmutableHashSet<string> set = handlerInterfaces
            .Concat(hostInterfaces)
            .ToImmutableHashSet();
        _logger.LogDebug(
            "Calculated allowed interfaces for command {Command}, found {InterfaceCount} interfaces",
            commandInfo,
            set.Count
        );
        return set;
    }

    private bool CanBeAccessedWithPrivilegeLevel(
        CommandInfo commandInfo,
        PrivilegeLevel privilegeLevel
    )
    {
        _logger.LogTrace(
            "Fetching privilege level for {CommandName}",
            commandInfo.PrimaryCommandName
        );
        PrivilegeLevel commandLevel = _cachedPrivilegeLevels.GetOrAdd(
            commandInfo,
            GetPrivilegeLevelFromCommand
        );
        _logger.LogTrace(
            "Comparing command {CommandName} privilege level {CommandPrivilegeLevel} against user privilege level {UserPrivilegeLevel}",
            commandInfo.PrimaryCommandName,
            commandLevel,
            privilegeLevel
        );
        return commandLevel <= privilegeLevel;
    }

    private PrivilegeLevel GetPrivilegeLevelFromCommand(CommandInfo commandInfo)
    {
        _logger.LogDebug("Calculating required privilege level for command {Command}", commandInfo);
        IEnumerable<PrivilegeLevel> handlerLevel = commandInfo
            .HandlerMethod.GetCustomAttributes<PrivilegeGuardAttribute>()
            .Select(a => a.MinLevel);
        IEnumerable<PrivilegeLevel> hostLevel = commandInfo
            .ImplementingType.GetCustomAttributes<PrivilegeGuardAttribute>()
            .Select(a => a.MinLevel);
        PrivilegeLevel privilegeLevel = handlerLevel
            .Concat(hostLevel)
            .Distinct()
            .OrderByDescending(i => i)
            .FirstOrDefault();

        _logger.LogDebug(
            "Calculated privilege level for {Command} is {PrivilegeLevel}",
            commandInfo,
            privilegeLevel
        );
        return privilegeLevel;
    }
}
