using System.Collections.Immutable;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.ListSupplier;

public class CommandListSupplierAggregator : ICommandListSupplier
{
    private readonly IImmutableList<ICommandListSupplier> _suppliers;

    public CommandListSupplierAggregator(IEnumerable<ICommandListSupplier> suppliers)
    {
        _suppliers = suppliers.ToImmutableList();
    }

    public IEnumerable<string> GetCommandList(CommandInstruction commandInstruction)
    {
        return _suppliers
            .SelectMany(s => s.GetCommandList(commandInstruction))
            .Distinct();
    }
}
