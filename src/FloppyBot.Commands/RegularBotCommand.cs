using System.Collections.Immutable;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands;

public abstract class RegularBotCommand : IBotCommand
{
    protected abstract IImmutableSet<string> CommandNames { get; }

    public virtual bool CanExecute(CommandInstruction instruction)
    {
        return CommandNames.Contains(instruction.CommandName);
    }

    public abstract ChatMessage? Execute(CommandInstruction instruction);
}
