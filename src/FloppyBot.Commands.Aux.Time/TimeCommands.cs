using System.Runtime.InteropServices;
using FloppyBot.Base.Clock;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
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

    private readonly ILogger<TimeCommands> _logger;
    private readonly ITimeProvider _timeProvider;

    public TimeCommands(
        ILogger<TimeCommands> logger,
        ITimeProvider timeProvider)
    {
        _logger = logger;
        _timeProvider = timeProvider;
    }

    private static TimeZoneInfo FindTimeZone(string? timeZoneId = null)
    {
        return !string.IsNullOrWhiteSpace(timeZoneId)
            ? FindTimeZoneWithLinuxId(timeZoneId)
            : TimeZoneInfo.Utc;
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

    [Command("time")]
    [CommandDescription("What is the current time")]
    public CommandResult ShowCurrentTime([AllArguments("_")] string? timeZoneId)
    {
        TimeZoneInfo timeZone;
        try
        {
            timeZone = FindTimeZone(timeZoneId);
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogError(ex, "Failed to get time zone {TimeZoneInput}", timeZoneId);
            return CommandResult.FailedWith(REPLY_ERR_TZ_NOT_FOUND.Format(new
            {
                Input = timeZoneId
            }));
        }

        DateTimeOffset currentUtcTime = _timeProvider.GetCurrentUtcTime();
        DateTimeOffset currentTimeAtTz = TimeZoneInfo.ConvertTime(currentUtcTime, timeZone);

        return REPLY_TIME.Format(new
        {
            Time = currentTimeAtTz,
            TimeZone = timeZone,
            TimeZoneName = timeZone.IsDaylightSavingTime(currentTimeAtTz)
                ? timeZone.DaylightName
                : timeZone.StandardName
        });
    }

    [Command("dectime", "dt")]
    [PrimaryCommandName("dectime")]
    [CommandDescription("What is the current decimal time")]
    public CommandResult ShowCurrentDecimalTime([AllArguments("_")] string? timeZoneId)
    {
        TimeZoneInfo timeZone;
        try
        {
            timeZone = FindTimeZone(timeZoneId);
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogError(ex, "Failed to get time zone {TimeZoneInput}", timeZoneId);
            return CommandResult.FailedWith(REPLY_ERR_TZ_NOT_FOUND.Format(new
            {
                Input = timeZoneId
            }));
        }

        DateTimeOffset currentUtcTime = _timeProvider.GetCurrentUtcTime();
        DateTimeOffset currentTimeAtTz = TimeZoneInfo.ConvertTime(currentUtcTime, timeZone);

        return REPLY_TIME_DEC.Format(new
        {
            TimeStr = DateTimeFormat.Format(new DateTime(currentTimeAtTz.DateTime), "HH:mm"),
            TimeZone = timeZone,
            TimeZoneName = timeZone.IsDaylightSavingTime(currentTimeAtTz)
                ? timeZone.DaylightName
                : timeZone.StandardName
        });
    }
}


