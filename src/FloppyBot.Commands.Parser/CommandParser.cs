using System.Collections.Immutable;
using FloppyBot.Base.EquatableCollections;

namespace FloppyBot.Commands.Parser;

public class CommandParser : ICommandParser
{
    private readonly IImmutableList<string> _supportedCommandPrefixes;

    public CommandParser(params string[] prefixes) : this(prefixes.ToImmutableListWithValueSemantics())
    {
    }

    public CommandParser(IImmutableList<string> supportedCommandPrefixes)
    {
        _supportedCommandPrefixes = supportedCommandPrefixes;
    }

    public IImmutableList<string> SupportedCommandPrefixes => _supportedCommandPrefixes;

    public CommandInstruction? ParseCommandFromString(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || !StringStartsWithAnyCommandPrefix(message))
        {
            return null;
        }

        string[] splitString = message.Split(' ');
        string commandName = splitString.First();
        IEnumerable<string> args = splitString.Length > 1 ? splitString.Skip(1) : Enumerable.Empty<string>();

        return new CommandInstruction(
            RemovePrefixFromCommandName(commandName),
            args.ToImmutableListWithValueSemantics());
    }

    private bool StringStartsWithAnyCommandPrefix(string message)
    {
        return _supportedCommandPrefixes
            .Any(message.StartsWith);
    }

    private string RemovePrefixFromCommandName(string commandWithPrefix)
    {
        string prefix = _supportedCommandPrefixes
            .First(commandWithPrefix.StartsWith);
        return commandWithPrefix[prefix.Length..];
    }
}
