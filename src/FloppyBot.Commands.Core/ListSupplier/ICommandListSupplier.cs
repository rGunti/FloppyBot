using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.ListSupplier;

public interface ICommandListSupplier
{
    IEnumerable<string> GetCommandList(CommandInstruction commandInstruction);
}
