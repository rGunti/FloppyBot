namespace FloppyBot.Base.Logging.MongoDb.Entities;

public class LogRecordSearchParameters
{
    public DateTimeOffset? MinTime { get; set; }
    public DateTimeOffset? MaxTime { get; set; }

    public LogLevel? MinLevel { get; set; }
    public LogLevel? MaxLevel { get; set; }

    public bool? HasException { get; set; }

    public string[]? Context { get; set; }
    public string[]? Service { get; set; }
    public string[]? InstanceName { get; set; }
    public string[]? MessageTemplate { get; set; }

    public string[]? ExcludeContext { get; set; }
    public string[]? ExcludeService { get; set; }
    public string[]? ExcludeInstanceName { get; set; }
    public string[]? ExcludeMessageTemplate { get; set; }

    public bool IncludeProperties { get; set; }

    public int MaxRecords { get; set; } = 100_000;
}
