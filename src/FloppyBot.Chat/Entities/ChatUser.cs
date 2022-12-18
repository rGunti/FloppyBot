using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Chat.Entities;

public record ChatUser(
    ChannelIdentifier Identifier,
    string DisplayName,
    PrivilegeLevel PrivilegeLevel)
{
    public static readonly ChatUser Anonymous = new ChatUser(
        "Anonymous/User",
        "Anonymous",
        PrivilegeLevel.Unknown);
}

