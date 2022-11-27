using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Spawner;

public interface ICommandSpawner
{
    ChatMessage? SpawnAndExecuteCommand(CommandInfo commandInfo, CommandInstruction instruction);
    bool CanExecuteVariableCommand(VariableCommandInfo commandInfo, CommandInstruction instruction);
}
