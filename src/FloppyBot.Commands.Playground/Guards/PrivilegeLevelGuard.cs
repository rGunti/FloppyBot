using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Playground.Guards;

public abstract class PrivilegeLevelGuard : ICommandGuard
{
    protected PrivilegeLevelGuard(PrivilegeLevel level)
    {
        Level = level;
    }

    private PrivilegeLevel Level { get; }

    public bool CanExecute(CommandInstruction instruction)
    {
        return instruction.Context != null && instruction.Context.SourceMessage.Author.PrivilegeLevel >= Level;
    }
}
