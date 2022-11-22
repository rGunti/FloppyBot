using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.ListSupplier;

public class BuiltInCommandListSupplier : ICommandListSupplier
{
    private readonly ICommandExecutor _commandExecutor;

    public BuiltInCommandListSupplier(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public IEnumerable<string> GetCommandList(CommandInstruction commandInstruction)
    {
        return _commandExecutor.KnownCommands
            .Select(c => c.PrimaryCommandName);
    }
}
