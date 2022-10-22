using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Chat.Entities;

public record ChatMessage(
    ChatMessageIdentifier Identifier,
    ChatUser Author,
    string EventName,
    string Content,
    string? Context = null,
    ChatInterfaceFeatures SupportedFeatures = ChatInterfaceFeatures.None);
