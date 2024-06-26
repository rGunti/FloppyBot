using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Cron;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Timer.Storage;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Aux.Timer;

[CommandHost]
public class TimerCommands
{
    public const string REPLY_CREATED =
        "Created new timer. Your message should be there in about {Time:time(hours minutes less)}.";

    public const string REPLY_FAILED_TIMESPAN =
        "Sorry, I couldn't understand when you wanted your reminder to arrive. Please repeat it again using this syntax: [<Days>d][<Hours>h][<Minutes>m]";

    private readonly ILogger<TimerCommands> _logger;
    private readonly ITimerService _timerService;
    private readonly IAuditor _auditor;

    public TimerCommands(
        ILogger<TimerCommands> logger,
        ITimerService timerService,
        IAuditor auditor
    )
    {
        _logger = logger;
        _timerService = timerService;
        _auditor = auditor;
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void DiSetup(IServiceCollection services)
    {
        services.AddTransient<ITimerService, TimerService>().AddCronJob<TimerCronJob>();
    }

    [Command("timer")]
    [CommandDescription("Creates a timer that sends a message when the given time is elapsed")]
    [CommandParameterHint(1, "time", CommandParameterType.String)]
    [CommandParameterHint(2, "message", CommandParameterType.String)]
    [PrivilegeGuard(PrivilegeLevel.Moderator)]
    public CommandResult CreateTimer(
        [ArgumentIndex(0)] string timeExpression,
        [ArgumentRange(1)] string timerMessage,
        [Author] ChatUser author,
        [SourceMessageIdentifier] ChatMessageIdentifier sourceMessageId,
        [SourceChannel] ChannelIdentifier sourceChannel
    )
    {
        TimeSpan? timespan = TimeExpressionParser.ParseTimeExpression(timeExpression);
        if (timespan == null)
        {
            return CommandResult.FailedWith(REPLY_FAILED_TIMESPAN);
        }

        _timerService.CreateTimer(sourceMessageId, author.Identifier, timespan.Value, timerMessage);
        _auditor.TimerCreated(author, sourceChannel, sourceMessageId, timerMessage, timespan.Value);
        return CommandResult.SuccessWith(REPLY_CREATED.Format(new { Time = timespan }));
    }
}
