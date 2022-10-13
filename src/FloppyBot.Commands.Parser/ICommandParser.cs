using System.Collections.Immutable;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Parser;

public interface ICommandParser
{
    IImmutableList<string> SupportedCommandPrefixes { get; }
    CommandInstruction? ParseCommandFromString(string message);
}
