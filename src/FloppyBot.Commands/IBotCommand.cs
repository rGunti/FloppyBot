using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands;

public interface IBotCommand
{
    bool CanExecute(CommandInstruction instruction);
    ChatMessage? Execute(CommandInstruction instruction);
}
