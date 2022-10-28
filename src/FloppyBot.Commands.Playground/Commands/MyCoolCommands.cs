using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Commands.Parser.Entities.Utils;
using FloppyBot.Commands.Playground.Attributes;
using FloppyBot.Commands.Playground.Guards;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Playground.Commands;

[CommandHost]
public class MyCoolCommands
{
    private readonly ILogger<MyCoolCommands> _logger;

    public MyCoolCommands(ILogger<MyCoolCommands> logger)
    {
        _logger = logger;
    }

    [Command("say")]
    [Guard(typeof(ModeratorGuard))]
    // ReSharper disable once UnusedMember.Global
    public ChatMessage? Say(
        CommandInstruction instruction,
        [CombinedParameter] string message)
    {
        _logger.LogInformation("Executing 'say' Command with {@Instruction}", instruction);
        if (!instruction.Parameters.Any())
        {
            return null;
        }

        return instruction.CreateReply(message);
    }

    [Command("ping")]
    // ReSharper disable once UnusedMember.Global
    public static ChatMessage? Ping(CommandInstruction instruction)
    {
        return instruction.CreateReply("Pong!");
    }
}
