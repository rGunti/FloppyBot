using System.Collections.Immutable;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.Executor;

public class CommandExecutor : ICommandExecutor
{
    private readonly ICommandSpawner _commandSpawner;
    private readonly ILogger<CommandExecutor> _logger;
    private readonly IImmutableDictionary<string, CommandInfo> _registeredCommands;

    public CommandExecutor(
        ILogger<CommandExecutor> logger,
        IImmutableDictionary<string, CommandInfo> registeredCommands,
        ICommandSpawner commandSpawner)
    {
        _logger = logger;
        _registeredCommands = registeredCommands;
        _commandSpawner = commandSpawner;
    }

    public ChatMessage? ExecuteCommand(CommandInstruction instruction)
    {
        var command = _registeredCommands.GetValueOrDefault(instruction.CommandName);
        if (command == null)
        {
            throw new KeyNotFoundException($"No command with name \"{instruction.CommandName}\" found");
        }

        _logger.LogDebug("Spawning and executing command {@Command}", command);
        return _commandSpawner.SpawnAndExecuteCommand(command, instruction);
    }
}
