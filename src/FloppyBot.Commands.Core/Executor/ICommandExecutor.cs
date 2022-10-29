using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Executor;

public interface ICommandExecutor
{
    ChatMessage? ExecuteCommand(CommandInstruction instruction);
}
