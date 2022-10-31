using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Guard;

public interface ICommandGuard
{
    bool CanExecute(CommandInstruction instruction, CommandInfo command, GuardAttribute settings);
}

public interface ICommandGuard<in TGuardAttribute> : ICommandGuard
    where TGuardAttribute : GuardAttribute
{
    bool CanExecute(CommandInstruction instruction, CommandInfo command, TGuardAttribute settings);
}
