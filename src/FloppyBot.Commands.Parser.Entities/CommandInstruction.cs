using System.Collections.Immutable;
using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Parser.Entities;

public record CommandInstruction(
    string CommandName,
    IImmutableList<string> Parameters,
    CommandContext? Context = null);

public record CommandContext(
    ChatMessage SourceMessage);
