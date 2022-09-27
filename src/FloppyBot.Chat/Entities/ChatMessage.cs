using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Chat.Entities;

public record ChatMessage(
    ChatMessageIdentifier Identifier,
    ChatUser Author,
    string Content,
    string? Context = null);
