using MongoDB.Bson;

namespace FloppyBot.Base.Logging.MongoDb.Entities;

internal class InternalLogRecord
{
    public ObjectId Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string UtcTimestamp { get; set; }
    public LogLevel Level { get; set; }
    public string MessageTemplate { get; set; }
    public string RenderedMessage { get; set; }
    public string? Exception { get; set; }
    public Dictionary<string, object> Properties { get; set; }

    public LogRecord ToLogRecord(bool includeProperties = false)
    {
        return new LogRecord(
            Id.ToString(),
            new DateTimeOffset(Timestamp, TimeSpan.Zero),
            Level,
            MessageTemplate,
            RenderedMessage,
            new LogRecordService(
                Properties.GetValueOrDefault("AssemblyName")?.ToString() ?? "unknown",
                Properties.GetValueOrDefault("AssemblyVersion")?.ToString() ?? "unknown",
                Properties.GetValueOrDefault("AssemblyInformationalVersion")?.ToString()
                    ?? "unknown",
                Properties.GetValueOrDefault("FloppyBotInstanceName")?.ToString()
            ),
            Properties.GetValueOrDefault("SourceContext")?.ToString() ?? "None",
            Exception,
            includeProperties ? ConvertPropertyDictionary() : null
        );
    }

    private Dictionary<string, string?> ConvertPropertyDictionary()
    {
        return Properties
            .Where(kvp =>
                kvp.Key != "AssemblyName"
                && kvp.Key != "AssemblyVersion"
                && kvp.Key != "AssemblyInformationalVersion"
                && kvp.Key != "FloppyBotInstanceName"
                && kvp.Key != "SourceContext"
            )
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
    }
}
