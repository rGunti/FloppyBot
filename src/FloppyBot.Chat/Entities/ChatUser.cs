using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Chat.Entities;

public record ChatUser(
    ChannelIdentifier Identifier,
    string DisplayName);
