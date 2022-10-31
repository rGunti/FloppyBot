using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Guard;

public class PrivilegeGuard : BaseGuard<PrivilegeGuardAttribute>
{
    public override bool CanExecute(
        CommandInstruction instruction,
        CommandInfo command,
        PrivilegeGuardAttribute settings)
    {
        return instruction.Context!.SourceMessage.Author.PrivilegeLevel >= settings.MinLevel;
    }
}
