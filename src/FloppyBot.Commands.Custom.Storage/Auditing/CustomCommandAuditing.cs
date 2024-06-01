using System.Diagnostics;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Commands.Custom.Storage.Auditing;

public static class CustomCommandActions
{
    public const string CounterUpdated = "CounterUpdated";
}

[StackTraceHidden]
public static class CustomCommandAuditing
{
    public const string CustomCommandType = "CustomCommand";

    public static void CommandCreated(
        this IAuditor auditor,
        ChatUser author,
        ChannelIdentifier channel,
        string commandName,
        string response
    )
    {
        auditor.Record(
            author.Identifier,
            channel,
            CustomCommandType,
            commandName,
            CommonActions.Created,
            response
        );
    }

    public static void CommandDeleted(
        this IAuditor auditor,
        ChatUser author,
        ChannelIdentifier channel,
        string commandName
    )
    {
        auditor.Record(
            author.Identifier,
            channel,
            CustomCommandType,
            commandName,
            CommonActions.Deleted
        );
    }

    public static void CounterUpdated(
        this IAuditor auditor,
        ChatUser author,
        ChannelIdentifier channel,
        string commandName,
        int value,
        int? increment = null
    )
    {
        var incrementStr = increment.HasValue
            ? $"({(increment > 0 ? "+" : string.Empty)}{increment})"
            : string.Empty;
        var detail = $"{value} {incrementStr}".Trim();
        auditor.Record(
            author.Identifier,
            channel,
            CustomCommandType,
            commandName,
            CustomCommandActions.CounterUpdated,
            detail
        );
    }
}
