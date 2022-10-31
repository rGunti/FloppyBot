using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Guard;

public abstract class BaseGuard<T> : ICommandGuard<T> where T : GuardAttribute
{
    public abstract bool CanExecute(CommandInstruction instruction, CommandInfo command, T settings);

    public bool CanExecute(CommandInstruction instruction, CommandInfo command, GuardAttribute settings)
    {
        return CanExecute(instruction, command, (T)settings);
    }
}
