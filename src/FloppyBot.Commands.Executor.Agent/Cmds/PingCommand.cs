using System.Collections.Immutable;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Executor.Agent.Cmds;

// ReSharper disable once ClassNeverInstantiated.Global
public class PingCommand : RegularBotCommand
{
    private static readonly IImmutableSet<string> CommandNameSet = new[] { "ping" }.ToImmutableHashSet();

    private readonly ILogger<PingCommand> _logger;

    public PingCommand(ILogger<PingCommand> logger)
    {
        _logger = logger;
    }

    protected override IImmutableSet<string> CommandNames => CommandNameSet;

    public override ChatMessage? Execute(CommandInstruction instruction)
    {
        _logger.LogInformation("Ping command executed by {UserId} with privilege level {PrivilegeLevel}",
            instruction.Context!.SourceMessage.Author.Identifier,
            instruction.Context!.SourceMessage.Author.PrivilegeLevel);
        return instruction.Context!.SourceMessage with
        {
            Content = "Pong!"
        };
    }
}
