using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Playground.Guards;

public interface ICommandGuard
{
    bool CanExecute(CommandInstruction instruction);
}
