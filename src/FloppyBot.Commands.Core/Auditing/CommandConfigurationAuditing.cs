using System.Diagnostics;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Config;

namespace FloppyBot.Commands.Core.Auditing;

[StackTraceHidden]
public static class CommandConfigurationAuditing
{
    public static void CommandConfigurationUpdated(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel,
        CommandConfiguration configuration
    )
    {
        auditor.Record(
            user.Identifier,
            channel,
            nameof(CommandConfiguration),
            configuration.CommandName,
            CommonActions.Updated,
            configuration.ToString()
        );
    }

    public static void CommandConfigurationDeleted(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel,
        string commandName
    )
    {
        auditor.Record(
            user.Identifier,
            channel,
            nameof(CommandConfiguration),
            commandName,
            CommonActions.Updated
        );
    }

    public static void CommandConfigurationDisabledSet(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel,
        string commandName,
        bool disabled
    )
    {
        auditor.Record(
            user.Identifier,
            channel,
            nameof(CommandConfiguration),
            commandName,
            disabled ? CommonActions.Disabled : CommonActions.Enabled
        );
    }
}
