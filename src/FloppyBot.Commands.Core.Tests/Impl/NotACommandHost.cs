using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Tests.Impl;

public class NotACommandHost
{
    [Command("nottobefound")]
    public ChatMessage? DoesNotExist(CommandInstruction _)
    {
        throw new NotImplementedException();
    }
}
