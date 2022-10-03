using System.Collections.Immutable;

namespace FloppyBot.Commands.Parser;

public record CommandInstruction(
    string CommandName,
    IImmutableList<string> Parameters);
