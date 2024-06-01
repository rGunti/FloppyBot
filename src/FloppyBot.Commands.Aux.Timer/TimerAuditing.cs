using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Commands.Aux.Timer;

public static class TimerAuditing
{
    public const string TimerType = "Timer";

    public static void TimerCreated(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel,
        string sourceMessageId,
        string timerMessage,
        TimeSpan timeSpan
    )
    {
        auditor.Record(
            user.Identifier,
            channel,
            TimerType,
            sourceMessageId,
            CommonActions.Created,
            $"[{timeSpan}]: {timerMessage}"
        );
    }
}
