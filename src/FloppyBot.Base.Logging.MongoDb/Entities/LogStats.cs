namespace FloppyBot.Base.Logging.MongoDb.Entities;

public record LogStats(
    int TotalCount,
    DateTimeOffset? OldestLogEntry,
    DateTimeOffset? NewestLogEntry
);
