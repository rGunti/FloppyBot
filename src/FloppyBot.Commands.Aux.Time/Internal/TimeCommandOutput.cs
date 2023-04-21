namespace FloppyBot.Commands.Aux.Time.Internal;

internal record TimeCommandOutput(
    DateTimeOffset Time,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string TimeStr,
    TimeZoneInfo TimeZone
)
{
    // ReSharper disable once UnusedMember.Global
    public DateTimeOffset UtcTime => TimeZoneInfo.ConvertTime(Time, TimeZoneInfo.Utc);

    // ReSharper disable once UnusedMember.Global
    public string TimeZoneName =>
        TimeZone.IsDaylightSavingTime(Time) ? TimeZone.DaylightName : TimeZone.StandardName;
}
