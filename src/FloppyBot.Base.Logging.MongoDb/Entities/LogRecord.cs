using System.Collections.Immutable;

namespace FloppyBot.Base.Logging.MongoDb.Entities;

public record LogRecord(
    string Id,
    DateTimeOffset Timestamp,
    LogLevel Level,
    string MessageTemplate,
    string RenderedMessage,
    LogRecordService Service,
    string Context,
    string? Exception,
    Dictionary<string, string?>? Properties
);

public record LogRecordService(
    string AssemblyName,
    string AssemblyVersion,
    string AssemblyInformationalVersion,
    string? InstanceName
);

public enum LogLevel
{
    Fatal,
    Error,
    Warning,
    Information,
    Debug,
    Verbose,
}

public static class LogLevels
{
    public static readonly IImmutableList<LogLevel> All = Enum.GetValues<LogLevel>()
        .ToImmutableArray();
}
