using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Commands.Parser.Entities.Utils;

namespace FloppyBot.Commands.Core.Tests.Impl;

[CommandHost]
public class SampleCommands
{
    [Command("ping", "test")]
    public ChatMessage? Ping(CommandInstruction instruction)
    {
        return instruction.CreateReply("Pong!");
    }

    [Command("sping")]
    public static ChatMessage? StaticPing(CommandInstruction instruction)
    {
        return instruction.CreateReply("Pong, but static!");
    }

    [Command("simple")]
    public string Simple(CommandInstruction _)
    {
        return "Simple Response";
    }

    [Command("noargs")]
    public static string NoArgsCommand()
    {
        return "No args at all";
    }
}
