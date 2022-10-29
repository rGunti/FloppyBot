using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Executor;

public interface ICommandSpawner
{
    ChatMessage? SpawnAndExecuteCommand(CommandInfo commandInfo, CommandInstruction instruction);
}
