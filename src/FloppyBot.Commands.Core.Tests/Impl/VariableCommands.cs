using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Entities;

namespace FloppyBot.Commands.Core.Tests.Impl;

[VariableCommandHost]
public class VariableCommands
{
    public bool CanHandle()
    {
        return true;
    }

    [VariableCommandHandler(nameof(CanHandle), "MyCustomHandler")]
    public CommandResult? HandleVariableCommands()
    {
        return null;
    }
}
