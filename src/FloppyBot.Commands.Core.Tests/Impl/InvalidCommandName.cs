using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Tests.Impl;

[CommandHost]
public class InvalidCommandName
{
    [Command(".NotSupported")]
    public ChatMessage? NotSupported(CommandInstruction _)
    {
        throw new NotImplementedException();
    }
}
