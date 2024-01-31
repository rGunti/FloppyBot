using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Guard;

public class SourceInterfaceGuard : BaseGuard<SourceInterfaceGuardAttribute>
{
    public override bool CanExecute(
        CommandInstruction instruction,
        CommandInfo command,
        SourceInterfaceGuardAttribute settings
    )
    {
        return settings.AllowedMessageInterfaces.Contains(
            instruction.Context!.SourceMessage.Identifier.Interface
        );
    }
}
