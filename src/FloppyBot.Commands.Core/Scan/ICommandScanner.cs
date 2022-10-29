using System.Collections.Immutable;
using FloppyBot.Commands.Core.Entities;

namespace FloppyBot.Commands.Core.Scan;

public interface ICommandScanner
{
    IImmutableDictionary<string, CommandInfo> ScanForCommandHandlers();
    IEnumerable<CommandInfo> ScanTypesForCommandHandlers(IEnumerable<Type> types);
    IEnumerable<CommandInfo> ScanTypeForCommandHandlers(Type type);
}
