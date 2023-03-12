using System.Runtime.InteropServices;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Time.Internal;
using FloppyBot.Commands.Aux.Time.Storage;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pallettaro.Revo;
using DateTime = Pallettaro.Revo.DateTime;

namespace FloppyBot.Commands.Aux.Time;

[CommandHost]
[CommandCategory("Time")]
// ReSharper disable once UnusedType.Global
public class TimeCommands
{
    private const string REPLY_TIME = "The current time is {Time:datetime:'HH:mm'} in {TimeZoneName}";
    private const string REPLY_TIME_DEC = "The current time is {TimeStr} DEC in {TimeZoneName}";
    private const string REPLY_ERR_TZ_NOT_FOUND = "Could not find the a timezone for \"{Input}\"";

    private const string REPLY_TZ_SET = "Your timezone is now set to {TimeZone}";
    private const string REPLY_ERR_TZ_NOT_DEFINED = "Run this command again and provide a timezone.";

    private readonly ILogger<TimeCommands> _logger;
    private readonly ITimeProvider _timeProvider;
    private readonly IUserTimeZoneSettingsService _userTimeZoneSettings;

    public TimeCommands(
        ILogger<TimeCommands> logger,
        ITimeProvider timeProvider,
        IUserTimeZoneSettingsService userTimeZoneSettings)
    {
        _logger = logger;
        _timeProvider = timeProvider;
        _userTimeZoneSettings = userTimeZoneSettings;
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void DiSetup(IServiceCollection services)
    {
        services
            .AddScoped<IUserTimeZoneSettingsService, UserTimeZoneSettingsService>();
    }

    private static TimeZoneInfo FindTimeZoneWithLinuxId(string timeZoneId)
    {
        string? tzId = timeZoneId;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            TimeZoneInfo.TryConvertIanaIdToWindowsId(tzId, out tzId);
        }

        if (tzId == null)
        {
            throw new TimeZoneNotFoundException(
                $"Could not convert IANA code {tzId} to Windows ID");
        }

        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    private TimeZoneInfo FindTimeZone(
        string author,
        string? timeZoneId = null)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            return FindTimeZoneWithLinuxId(timeZoneId);
        }

        return _userTimeZoneSettings.GetTimeZoneForUser(author)
            .Select(s => FindTimeZoneWithLinuxId(s.TimeZoneId))
            .FirstOrDefault(TimeZoneInfo.Utc);
    }

    private NullableObject<TimeCommandOutput> GetTime(string author, string? timeZoneId)
    {
        TimeZoneInfo timeZone;
        try
        {
            timeZone = FindTimeZone(author, timeZoneId);
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogError(ex, "Failed to get time zone {TimeZoneInput}", timeZoneId);
            return NullableObject.Empty<TimeCommandOutput>();
        }

        DateTimeOffset currentUtcTime = _timeProvider.GetCurrentUtcTime();
        DateTimeOffset currentTimeAtTz = TimeZoneInfo.ConvertTime(currentUtcTime, timeZone);

        return new TimeCommandOutput(
            currentTimeAtTz,
            currentTimeAtTz.ToString("HH:mm"),
            timeZone);
    }

    [Command("time")]
    [CommandDescription("What is the current time")]
    [CommandParameterHint(1, "timeZone", CommandParameterType.String, false)]
    public CommandResult ShowCurrentTime(
        [Author] ChatUser author,
        [AllArguments("_")]
        string? timeZoneId)
    {
        return GetTime(author.Identifier, timeZoneId)
            .Select(t => REPLY_TIME.Format(t))
            .Select(CommandResult.SuccessWith)
            .FirstOrDefault(CommandResult.FailedWith(REPLY_ERR_TZ_NOT_FOUND.Format(new
            {
                Input = timeZoneId
            })));
    }

    [Command("dectime", "dt")]
    [PrimaryCommandName("dectime")]
    [CommandDescription("What is the current decimal time")]
    [CommandParameterHint(1, "timeZone", CommandParameterType.String, false)]
    public CommandResult ShowCurrentDecimalTime(
        [Author] ChatUser author,
        [AllArguments("_")]
        string? timeZoneId)
    {
        return GetTime(author.Identifier, timeZoneId)
            .Select(t => REPLY_TIME_DEC.Format(t with
            {
                TimeStr = DateTimeFormat.Format(new DateTime(t.Time.DateTime), "HH:mm")
            }))
            .Select(CommandResult.SuccessWith)
            .FirstOrDefault(CommandResult.FailedWith(REPLY_ERR_TZ_NOT_FOUND.Format(new
            {
                Input = timeZoneId
            })));
    }

    [Command("timeset")]
    [CommandDescription("Set your default timezone")]
    [PrivilegeGuard(PrivilegeLevel.Viewer)]
    [CommandParameterHint(1, "timeZone", CommandParameterType.String)]
    public CommandResult SetUserTimeZone(
        [Author] ChatUser author,
        [AllArguments("_")]
        string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return CommandResult.FailedWith(REPLY_ERR_TZ_NOT_DEFINED);
        }

        TimeZoneInfo timeZoneInfo;
        try
        {
            timeZoneInfo = FindTimeZoneWithLinuxId(timeZoneId);
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogError(ex, "Failed to get time zone {TimeZoneInput}", timeZoneId);
            return CommandResult.FailedWith(REPLY_ERR_TZ_NOT_FOUND.Format(new
            {
                Input = timeZoneId
            }));
        }

        _userTimeZoneSettings.SetTimeZoneForUser(author.Identifier, timeZoneInfo.Id);
        return CommandResult.SuccessWith(REPLY_TZ_SET.Format(new
        {
            TimeZone = timeZoneInfo.Id
        }));
    }
}
