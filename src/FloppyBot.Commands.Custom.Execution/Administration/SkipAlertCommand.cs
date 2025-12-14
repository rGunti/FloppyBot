using FloppyBot.Base.Clock;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Custom.Communication;
using FloppyBot.Commands.Custom.Communication.Entities;

namespace FloppyBot.Commands.Custom.Execution.Administration;

[CommandHost]
[CommandCategory("Custom Commands")]
[PrivilegeGuard(PrivilegeLevel.Moderator)]
// ReSharper disable once UnusedType.Global
public class SkipAlertCommand
{
    private const string REPLY_ALERT_SKIPPED = "Alert has been skipped.";

    private readonly ISoundCommandInvocationSender _commandInvocationSender;
    private readonly ITimeProvider _timeProvider;

    public SkipAlertCommand(
        ISoundCommandInvocationSender commandInvocationSender,
        ITimeProvider timeProvider
    )
    {
        _commandInvocationSender = commandInvocationSender;
        _timeProvider = timeProvider;
    }

    [Command("skipalert", "skip")]
    [PrimaryCommandName("skipalert")]
    [CommandDescription("Skips the alert for a custom command")]
    public CommandResult SkipAlert([SourceChannel] string sourceChannel, [Author] ChatUser author)
    {
        _commandInvocationSender.InvokeSoundCommand(
            new SoundCommandInvocation(
                PayloadType.Command,
                author.Identifier,
                sourceChannel,
                "skip",
                string.Empty,
                _timeProvider.GetCurrentUtcTime()
            )
        );
        return CommandResult.SuccessWith(REPLY_ALERT_SKIPPED);
    }
}
