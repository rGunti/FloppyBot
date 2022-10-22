using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Chat.Mock;

public static class MockMessageFactory
{
    public static ChatMessageIdentifier NewChatMessageIdentifier(
        string channelName = "Channel")
    {
        return new ChatMessageIdentifier(
            MockChatInterface.IF_NAME,
            channelName,
            Guid.NewGuid().ToString());
    }

    public static ChannelIdentifier NewChatUserIdentifier(
        string username = "UserName")
    {
        return new ChannelIdentifier(
            MockChatInterface.IF_NAME,
            username);
    }

    public static ChatUser NewChatUser(
        string userId,
        string? userName = null,
        PrivilegeLevel privilegeLevel = PrivilegeLevel.Unknown)
    {
        return new ChatUser(
            NewChatUserIdentifier(userId),
            userName ?? userId,
            privilegeLevel);
    }

    public static ChatMessage NewChatMessage(
        string message,
        ChatMessageIdentifier? messageId = null,
        ChatUser? user = null,
        string sourceChannel = "Channel")
    {
        return new ChatMessage(
            messageId ?? NewChatMessageIdentifier(sourceChannel),
            user ?? NewChatUser("ChatUser"),
            SharedEventTypes.CHAT_MESSAGE,
            message);
    }
}
