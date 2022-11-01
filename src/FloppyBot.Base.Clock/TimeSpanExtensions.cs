namespace FloppyBot.Base.Clock;

public static class TimeSpanExtensions
{
    public static TimeSpan Millisecond(this int i) => TimeSpan.FromMilliseconds(i);
    public static TimeSpan Milliseconds(this int i) => TimeSpan.FromMilliseconds(i);
    public static TimeSpan Second(this int i) => TimeSpan.FromSeconds(i);
    public static TimeSpan Seconds(this int i) => TimeSpan.FromSeconds(i);
    public static TimeSpan Minute(this int i) => TimeSpan.FromMinutes(i);
    public static TimeSpan Minutes(this int i) => TimeSpan.FromMinutes(i);
    public static TimeSpan Hour(this int i) => TimeSpan.FromHours(i);
    public static TimeSpan Hours(this int i) => TimeSpan.FromHours(i);
    public static TimeSpan Day(this int i) => TimeSpan.FromDays(i);
    public static TimeSpan Days(this int i) => TimeSpan.FromDays(i);

    public static int SecondInMs(this int i) => (int)TimeSpan.FromSeconds(i).TotalMilliseconds;
    public static int SecondsInMs(this int i) => (int)TimeSpan.FromSeconds(i).TotalMilliseconds;
    public static int MinuteInMs(this int i) => (int)TimeSpan.FromMinutes(i).TotalMilliseconds;
    public static int MinutesInMs(this int i) => (int)TimeSpan.FromMinutes(i).TotalMilliseconds;
    public static int HourInMs(this int i) => (int)TimeSpan.FromHours(i).TotalMilliseconds;
    public static int HoursInMs(this int i) => (int)TimeSpan.FromHours(i).TotalMilliseconds;
    public static int DayInMs(this int i) => (int)TimeSpan.FromDays(i).TotalMilliseconds;
    public static int DaysInMs(this int i) => (int)TimeSpan.FromDays(i).TotalMilliseconds;
}
