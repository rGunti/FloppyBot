using System.Collections.Immutable;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Spawner;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.Executor;

public class CommandExecutor : ICommandExecutor
{
    private readonly ICommandSpawner _commandSpawner;
    private readonly ILogger<CommandExecutor> _logger;
    private readonly IImmutableDictionary<string, CommandInfo> _registeredCommands;
    private readonly IImmutableList<VariableCommandInfo> _variableCommands;

    public CommandExecutor(
        ILogger<CommandExecutor> logger,
        IImmutableDictionary<string, CommandInfo> registeredCommands,
        IImmutableList<VariableCommandInfo> variableCommands,
        ICommandSpawner commandSpawner)
    {
        _logger = logger;
        _registeredCommands = registeredCommands;
        _variableCommands = variableCommands;
        _commandSpawner = commandSpawner;
    }

    public IEnumerable<CommandInfo> KnownCommands
        => _registeredCommands
            .Values
            .Distinct();

    public ChatMessage? ExecuteCommand(CommandInstruction instruction)
    {
        CommandInfo? command = _registeredCommands.GetValueOrDefault(instruction.CommandName);
        if (command != null)
        {
            _logger.LogDebug("Spawning and executing command {@Command}", command);
            return _commandSpawner.SpawnAndExecuteCommand(command, instruction);
        }

        _logger.LogDebug(
            "Preregistered Command with name {CommandName} could not be found, trying to find a handler to execute it",
            instruction.CommandName);

        VariableCommandInfo? availableHandlers = _variableCommands
            .Select(handler => new VariableCommandInfo(instruction.CommandName, handler))
            .FirstOrDefault(handler => _commandSpawner.CanExecuteVariableCommand(handler, instruction));
        if (availableHandlers != null)
        {
            _logger.LogDebug("Spawning and executing variable command {@Command}", command);
            return _commandSpawner.SpawnAndExecuteCommand(availableHandlers, instruction);
        }

        throw new KeyNotFoundException($"No command with name \"{instruction.CommandName}\" found");
    }
}
