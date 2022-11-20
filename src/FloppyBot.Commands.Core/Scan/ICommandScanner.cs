using System.Collections.Immutable;
using FloppyBot.Commands.Core.Entities;

namespace FloppyBot.Commands.Core.Scan;

public interface ICommandScanner
{
    IImmutableDictionary<string, CommandInfo> ScanForCommandHandlers();
    IEnumerable<VariableCommandInfo> ScanForVariableCommandHandlers();
    IImmutableDictionary<string, CommandInfo> IndexCommands(IEnumerable<CommandInfo> commands);
    IEnumerable<CommandInfo> ScanTypesForCommandHandlers(IEnumerable<Type> types);
    IEnumerable<CommandInfo> ScanTypeForCommandHandlers(Type type);
    IEnumerable<VariableCommandInfo> ScanTypesForVariableCommandHandlers(IEnumerable<Type> types);
    IEnumerable<VariableCommandInfo> ScanTypeForVariableCommandHandlers(Type type);
}
