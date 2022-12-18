using System.Collections.Immutable;
using FloppyBot.Aux.MessageCounter.Core;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Cron;
using FloppyBot.Base.Cron.Attributes;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Twitch.Storage;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;
using FloppyBot.Commands.Core.Replier;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Aux.Twitch;

[CronInterval(Milliseconds = 30000)]
public class TimerMessageCronJob : ICronJob
{
    private readonly ITimerMessageConfigurationService _configurationService;
    private readonly ILogger<TimerMessageCronJob> _logger;
    private readonly IMessageOccurrenceService _messageOccurrenceService;
    private readonly IMessageReplier _messageReplier;
    private readonly ITimeProvider _timeProvider;

    public TimerMessageCronJob(
        ILogger<TimerMessageCronJob> logger,
        IMessageOccurrenceService messageOccurrenceService,
        IMessageReplier messageReplier,
        ITimerMessageConfigurationService configurationService,
        ITimeProvider timeProvider)
    {
        _logger = logger;
        _messageOccurrenceService = messageOccurrenceService;
        _messageReplier = messageReplier;
        _configurationService = configurationService;
        _timeProvider = timeProvider;
    }

    public void Run()
    {
        var messagesToSend = _configurationService.GetAllConfigs()
            .Select(config => new
            {
                Config = config,
                LastExecution = _configurationService.GetLastExecution(config.Id)
                    .SingleOrDefault(new TimerMessageExecution(
                        config.Id,
                        DateTimeOffset.MinValue,
                        -1)),
                MessageCount = config.MinMessages > 0
                    ? _messageOccurrenceService.GetMessageCountInChannel(
                        config.Id,
                        TimeSpan.FromMinutes(config.MinMessages))
                    : -1
            })
            .Where(i => IsExecutionRequired(i.Config, i.LastExecution, i.MessageCount))
            .Select(i => CreateChatMessage(i.Config, i.LastExecution))
            .ToImmutableArray();

        _logger.LogInformation(
            "Got {TimerMessageCount} timer messages to send",
            messagesToSend.Length);

        foreach (ChatMessage message in messagesToSend)
        {
            _messageReplier.SendMessage(message);
        }
    }

    private bool IsExecutionRequired(
        TimerMessageConfiguration timerMessageConfiguration,
        TimerMessageExecution lastExecution,
        int messageCount)
    {
        _logger.LogDebug("Testing if timer for channel {ChannelId} is to be executed", timerMessageConfiguration.Id);

        DateTimeOffset now = _timeProvider.GetCurrentUtcTime();
        TimeSpan interval = TimeSpan.FromMinutes(timerMessageConfiguration.Interval);

        DateTimeOffset expectedExecutionTime = lastExecution.LastExecutedAt + interval;
        if (now < expectedExecutionTime)
        {
            _logger.LogTrace("Time is not up yet for {ChannelId}", timerMessageConfiguration.Id);
            return false;
        }

        _logger.LogTrace(
            "{ChannelId} received {MessageCount} messages in the last {Interval}",
            timerMessageConfiguration.Id,
            messageCount,
            interval);
        return
            timerMessageConfiguration.MinMessages <= 0 ||
            messageCount >= timerMessageConfiguration.MinMessages;
    }

    private ChatMessage CreateChatMessage(TimerMessageConfiguration config, TimerMessageExecution lastExecution)
    {
        var msgIndex = (lastExecution.MessageIndex + 1) % config.Messages.Length;
        _configurationService.UpdateLastExecutionTime(
            config.Id,
            _timeProvider.GetCurrentUtcTime(),
            msgIndex);
        return CreateMessageTo(config.Id, config.Messages[msgIndex]);
    }

    private static ChatMessage CreateMessageTo(string channelId, string content)
    {
        return new ChatMessage(
            ChatMessageIdentifier.NewFor(channelId),
            ChatUser.Anonymous,
            SharedEventTypes.CHAT_MESSAGE,
            content);
    }
}


