using System.Collections.Immutable;
using FloppyBot.Base.Cron;
using FloppyBot.Base.Cron.Attributes;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Timer.Storage;
using FloppyBot.Commands.Aux.Timer.Storage.Entities;
using FloppyBot.Commands.Core.Replier;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Aux.Timer;

[CronInterval(Milliseconds = 30000)]
public class TimerCronJob : ICronJob
{
    private const string EMOJI_SEQ_BELL = "f09f9494";
    private static readonly string BellEmoji = EMOJI_SEQ_BELL.ConvertEmojiSequence();

    private readonly ILogger<TimerCronJob> _logger;
    private readonly IMessageReplier _replier;
    private readonly ITimerService _timerService;

    public TimerCronJob(
        ILogger<TimerCronJob> logger,
        ITimerService timerService,
        IMessageReplier replier
    )
    {
        _logger = logger;
        _timerService = timerService;
        _replier = replier;
    }

    public void Run()
    {
        _logger.LogTrace("Checking for expired timers");
        ImmutableArray<TimerRecord> expiredTimers = _timerService
            .GetExpiredTimers(false)
            .ToImmutableArray();

        if (!expiredTimers.Any())
        {
            _logger.LogTrace("No expired timers found, we're done");
            return;
        }

        _logger.LogInformation(
            "Sending messages for {TimerCount} expired timers",
            expiredTimers.Length
        );
        foreach (ChatMessage message in expiredTimers.Select(ConvertToMessage))
        {
            _logger.LogTrace("Sending timer message for {MessageIdentifier}", message.Identifier);
            _replier.SendMessage(message);
        }

        _logger.LogInformation("Deleting {TimerCount} expired timers", expiredTimers.Length);
        _timerService.DeleteTimers(expiredTimers);
    }

    private static ChatMessage ConvertToMessage(TimerRecord timerRecord)
    {
        return new ChatMessage(
            timerRecord.SourceMessage,
            new ChatUser(timerRecord.CreatedBy, string.Empty, PrivilegeLevel.Unknown),
            SharedEventTypes.CHAT_MESSAGE,
            $"{BellEmoji} {timerRecord.TimerMessage}"
        );
    }
}
