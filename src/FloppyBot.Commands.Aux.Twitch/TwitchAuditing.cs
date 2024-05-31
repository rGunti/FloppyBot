using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;

namespace FloppyBot.Commands.Aux.Twitch;

public static class TwitchAuditing
{
    public const string ShoutoutMessageType = "ShoutoutMessage";

    public static void ShoutoutMessageSet(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel,
        ShoutoutMessageSetting message
    )
    {
        auditor.Record(
            user.Identifier,
            channel,
            ShoutoutMessageType,
            string.Empty,
            CommonActions.Updated,
            message.ToString()
        );
    }

    public static void ShoutoutMessageSet(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel,
        string message
    )
    {
        auditor.Record(
            user.Identifier,
            channel,
            ShoutoutMessageType,
            nameof(ShoutoutMessageSetting.Message),
            CommonActions.Updated,
            message
        );
    }

    public static void TeamShoutoutMessageSet(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel,
        string message
    )
    {
        auditor.Record(
            user.Identifier,
            channel,
            ShoutoutMessageType,
            nameof(ShoutoutMessageSetting.TeamMessage),
            CommonActions.Updated,
            message
        );
    }

    public static void ShoutoutMessageCleared(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel
    )
    {
        auditor.Record(
            user.Identifier,
            channel,
            ShoutoutMessageType,
            string.Empty,
            CommonActions.Deleted
        );
    }
}
