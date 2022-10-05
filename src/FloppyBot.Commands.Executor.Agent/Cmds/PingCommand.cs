using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Executor.Agent.Cmds;

public class PingCommand : IBotCommand
{
    private readonly ILogger<PingCommand> _logger;

    public PingCommand(ILogger<PingCommand> logger)
    {
        _logger = logger;
    }

    public bool CanExecute(CommandInstruction instruction)
    {
        return instruction.CommandName == "ping";
    }

    public ChatMessage? Execute(CommandInstruction instruction)
    {
        _logger.LogInformation("Ping command executed by {UserId} with privilege level {PrivilegeLevel}",
            instruction.Context!.SourceMessage.Author.Identifier,
            instruction.Context!.SourceMessage.Author.PrivilegeLevel);
        return null;
    }
}
