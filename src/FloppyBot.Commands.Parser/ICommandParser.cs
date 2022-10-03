using System.Collections.Immutable;

namespace FloppyBot.Commands.Parser;

public interface ICommandParser
{
    IImmutableList<string> SupportedCommandPrefixes { get; }
    CommandInstruction? ParseCommandFromString(string message);
}
