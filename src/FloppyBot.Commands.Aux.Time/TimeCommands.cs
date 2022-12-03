using System.Runtime.InteropServices;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using Microsoft.Extensions.Logging;
using Pallettaro.Revo;
using DateTime = Pallettaro.Revo.DateTime;

namespace FloppyBot.Commands.Aux.Time;

internal record TimeCommandOutput(
    DateTimeOffset Time,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string TimeStr,
    TimeZoneInfo TimeZone)
{
    // ReSharper disable once UnusedMember.Global
    public DateTimeOffset UtcTime => TimeZoneInfo.ConvertTime(Time, TimeZoneInfo.Utc);

    // ReSharper disable once UnusedMember.Global
    public string TimeZoneName
        => TimeZone.IsDaylightSavingTime(Time)
            ? TimeZone.DaylightName
            : TimeZone.StandardName;
}

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

    private NullableObject<TimeCommandOutput> GetTime(string? timeZoneId)
    {
        TimeZoneInfo timeZone;
        try
        {
            timeZone = FindTimeZone(timeZoneId);
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
    public CommandResult ShowCurrentTime([AllArguments("_")] string? timeZoneId)
    {
        return GetTime(timeZoneId)
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
    public CommandResult ShowCurrentDecimalTime([AllArguments("_")] string? timeZoneId)
    {
        return GetTime(timeZoneId)
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
}



