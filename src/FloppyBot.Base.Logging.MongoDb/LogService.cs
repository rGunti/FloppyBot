using FloppyBot.Base.Logging.MongoDb.Entities;
using MongoDB.Driver;

namespace FloppyBot.Base.Logging.MongoDb;

public class LogService
{
    private const int MAX_RECORDS = 10_000;

    private static readonly TimeSpan MaxTimeSpan = TimeSpan.FromDays(14);

    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<InternalLogRecord> _collection;
    private readonly TimeProvider _timeProvider;

    public LogService(IMongoDatabase database, TimeProvider timeProvider)
    {
        _database = database;
        _timeProvider = timeProvider;
        _collection = database.GetCollection<InternalLogRecord>("_Logs");
    }

    public IEnumerable<LogRecord> GetLog()
    {
        return _collection
            .AsQueryable()
            .OrderByDescending(l => l.Timestamp)
            .Take(10_000)
            .ToList()
            .Select(l => l.ToLogRecord());
    }

    public IEnumerable<LogRecord> GetLog(LogRecordSearchParameters searchParams)
    {
        var (minTime, maxTime) = GetTimeFilter(searchParams);

        var filterBuilder = Builders<InternalLogRecord>.Filter;
        List<FilterDefinition<InternalLogRecord>> filters =
        [
            filterBuilder.Gte(l => l.Timestamp, minTime),
            filterBuilder.Lte(l => l.Timestamp, maxTime)
        ];

        if (searchParams.MinLevel is not null || searchParams.MaxLevel is not null)
        {
            filters.Add(
                filterBuilder.In(
                    l => l.Level,
                    GetFilters(searchParams.MinLevel, searchParams.MaxLevel)
                )
            );
        }

        if (searchParams.HasException.HasValue)
        {
            filters.Add(filterBuilder.Exists(l => l.Exception, searchParams.HasException.Value));
        }

        if (searchParams.Context is { Length: > 0 })
        {
            filters.Add(filterBuilder.In(l => l.Properties["SourceContext"], searchParams.Context));
        }

        if (searchParams.ExcludeContext is { Length: > 0 })
        {
            filters.Add(
                filterBuilder.Nin(l => l.Properties["SourceContext"], searchParams.ExcludeContext)
            );
        }

        if (searchParams.Service is { Length: > 0 })
        {
            filters.Add(filterBuilder.In(l => l.Properties["AssemblyName"], searchParams.Service));
        }

        if (searchParams.ExcludeService is { Length: > 0 })
        {
            filters.Add(
                filterBuilder.Nin(l => l.Properties["AssemblyName"], searchParams.ExcludeService)
            );
        }

        if (searchParams.InstanceName is { Length: > 0 })
        {
            filters.Add(
                filterBuilder.In(
                    l => l.Properties["FloppyBotInstanceName"],
                    searchParams.InstanceName
                )
            );
        }

        if (searchParams.ExcludeInstanceName is { Length: > 0 })
        {
            filters.Add(
                filterBuilder.Nin(
                    l => l.Properties["FloppyBotInstanceName"],
                    searchParams.ExcludeInstanceName
                )
            );
        }

        if (searchParams.MessageTemplate is { Length: > 0 })
        {
            filters.Add(filterBuilder.In(l => l.MessageTemplate, searchParams.MessageTemplate));
        }

        return _collection
            .FindSync(filterBuilder.And(filters), GetFindOptions(searchParams))
            .ToList()
            .Select(l => l.ToLogRecord(searchParams.IncludeProperties));
    }

    private static IEnumerable<LogLevel> GetFilters(LogLevel? minLevel, LogLevel? maxLevel)
    {
        return LogLevels.All.Where(l =>
            l <= (minLevel ?? LogLevel.Verbose) && l >= (maxLevel ?? LogLevel.Fatal)
        );
    }

    private static FindOptions<InternalLogRecord> GetFindOptions(
        LogRecordSearchParameters searchParams
    )
    {
        return new FindOptions<InternalLogRecord>
        {
            Limit = Math.Max(1, Math.Min(searchParams.MaxRecords, MAX_RECORDS)),
            Sort = Builders<InternalLogRecord>.Sort.Descending(l => l.Timestamp),
        };
    }

    private (DateTime From, DateTime To) GetTimeFilter(LogRecordSearchParameters searchParams)
    {
        return (
            DetermineMinTime(searchParams.MinTime, searchParams.MaxTime),
            DetermineMaxTime(searchParams.MaxTime)
        );
    }

    private DateTime DetermineMinTime(DateTimeOffset? minTime, DateTimeOffset? maxTime)
    {
        if (minTime is not null)
        {
            return minTime.Value.UtcDateTime;
        }

        return ((maxTime ?? _timeProvider.GetUtcNow()) - MaxTimeSpan).UtcDateTime;
    }

    private DateTime DetermineMaxTime(DateTimeOffset? maxTime)
    {
        return (maxTime ?? _timeProvider.GetUtcNow()).UtcDateTime;
    }
}
