using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Playground.Guards;

public class DisabledGuard : ICommandGuard
{
    public bool CanExecute(CommandInstruction instruction)
    {
        return false;
    }
}
