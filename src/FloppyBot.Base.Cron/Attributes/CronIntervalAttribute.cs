namespace FloppyBot.Base.Cron.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CronIntervalAttribute : Attribute
{
    public int Milliseconds { get; set; }
    public bool RunOnStartup { get; set; }
}
