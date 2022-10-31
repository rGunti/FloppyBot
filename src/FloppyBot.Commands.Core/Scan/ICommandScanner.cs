using System.Collections.Immutable;
using FloppyBot.Commands.Core.Entities;

namespace FloppyBot.Commands.Core.Scan;

public interface ICommandScanner
{
    IImmutableDictionary<string, CommandInfo> ScanForCommandHandlers();
    IImmutableDictionary<string, CommandInfo> IndexCommands(IEnumerable<CommandInfo> commands);
    IEnumerable<CommandInfo> ScanTypesForCommandHandlers(IEnumerable<Type> types);
    IEnumerable<CommandInfo> ScanTypeForCommandHandlers(Type type);
}
