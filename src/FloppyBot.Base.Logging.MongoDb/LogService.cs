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

    private static FilterDefinitionBuilder<InternalLogRecord> Filter =>
        Builders<InternalLogRecord>.Filter;

    private static ProjectionDefinitionBuilder<InternalLogRecord> Projection =>
        Builders<InternalLogRecord>.Projection;

    public IEnumerable<LogRecord> GetLog(LogRecordSearchParameters searchParams)
    {
        return _collection
            .FindSync(Filter.And(GetFilter(searchParams)), GetFindOptions(searchParams))
            .ToList()
            .Select(l => l.ToLogRecord(searchParams.IncludeProperties));
    }

    public LogStats GetLogStats(LogRecordSearchParameters searchParameters)
    {
        var filters = GetFilter(searchParameters).ToList();
        var aggregation = _collection.Aggregate();
        if (filters.Count != 0)
        {
            aggregation = aggregation.Match(Filter.And(filters));
        }

        var data = aggregation
            .Group(
                l => true,
                g => new
                {
                    Count = g.Count(),
                    Oldest = g.Min(l => l.Timestamp),
                    Newest = g.Max(l => l.Timestamp),
                }
            )
            .ToList();
        return data.Select(d => new LogStats(d.Count, d.Oldest, d.Newest))
            .FirstOrDefault(new LogStats(0, null, null));
    }

    private static IEnumerable<LogLevel> GetLogLevelsToFilterFor(
        LogLevel? minLevel,
        LogLevel? maxLevel
    )
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

    private IEnumerable<FilterDefinition<InternalLogRecord>> GetFilter(
        LogRecordSearchParameters searchParams
    )
    {
        var (minTime, maxTime) = GetTimeFilter(searchParams);
        yield return Filter.Gte(l => l.Timestamp, minTime);
        yield return Filter.Lte(l => l.Timestamp, maxTime);

        if (searchParams.MinLevel is not null || searchParams.MaxLevel is not null)
        {
            yield return Filter.In(
                l => l.Level,
                GetLogLevelsToFilterFor(searchParams.MinLevel, searchParams.MaxLevel)
            );
        }

        if (searchParams.HasException.HasValue)
        {
            yield return Filter.Exists(l => l.Exception, searchParams.HasException.Value);
        }

        if (searchParams.Context is { Length: > 0 })
        {
            yield return Filter.In(l => l.Properties["SourceContext"], searchParams.Context);
        }

        if (searchParams.ExcludeContext is { Length: > 0 })
        {
            yield return Filter.Nin(
                l => l.Properties["SourceContext"],
                searchParams.ExcludeContext
            );
        }

        if (searchParams.Service is { Length: > 0 })
        {
            yield return Filter.In(l => l.Properties["AssemblyName"], searchParams.Service);
        }

        if (searchParams.ExcludeService is { Length: > 0 })
        {
            yield return Filter.Nin(l => l.Properties["AssemblyName"], searchParams.ExcludeService);
        }

        if (searchParams.InstanceName is { Length: > 0 })
        {
            yield return Filter.In(
                l => l.Properties["FloppyBotInstanceName"],
                searchParams.InstanceName
            );
        }

        if (searchParams.ExcludeInstanceName is { Length: > 0 })
        {
            yield return Filter.Nin(
                l => l.Properties["FloppyBotInstanceName"],
                searchParams.ExcludeInstanceName
            );
        }

        if (searchParams.MessageTemplate is { Length: > 0 })
        {
            yield return Filter.In(l => l.MessageTemplate, searchParams.MessageTemplate);
        }

        if (searchParams.ExcludeMessageTemplate is { Length: > 0 })
        {
            yield return Filter.Nin(l => l.MessageTemplate, searchParams.ExcludeMessageTemplate);
        }
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
