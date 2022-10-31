using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Executor;

public interface ICommandExecutor
{
    IEnumerable<CommandInfo> KnownCommands { get; }
    ChatMessage? ExecuteCommand(CommandInstruction instruction);
}
