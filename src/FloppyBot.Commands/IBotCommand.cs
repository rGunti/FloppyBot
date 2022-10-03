using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser;

namespace FloppyBot.Commands;

public interface IBotCommand
{
    ChatMessage? Execute(ChatMessage message, CommandInstruction instruction);
}
