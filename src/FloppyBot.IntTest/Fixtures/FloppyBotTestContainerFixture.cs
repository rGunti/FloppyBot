using DotNet.Testcontainers.Containers;
using MongoDB.Driver;
using StackExchange.Redis;
using Testcontainers.MongoDb;
using Testcontainers.Redis;

namespace FloppyBot.IntTest.Fixtures;

[CollectionDefinition(NAME)]
public class FloppyBotTestCollection : ICollectionFixture<FloppyBotTestContainerFixture>
{
    public const string NAME = "FloppyBot";
}

public class FloppyBotTestContainerFixture : IAsyncDisposable
{
    private const string MONGO_USERNAME = "floppybot";
    private const string MONGO_PASSWORD = "floppytest";

    private readonly IContainer _mongoDb;
    private readonly Lazy<IMongoClient> _mongoClient;
    private readonly IContainer _redis;
    private readonly Lazy<IConnectionMultiplexer> _redisMultiplexer;

    public FloppyBotTestContainerFixture()
    {
        _mongoDb = new MongoDbBuilder()
            .WithUsername(MONGO_USERNAME)
            .WithPassword(MONGO_PASSWORD)
            .WithPortBinding(27017, true)
            .Build();
        _mongoClient = new Lazy<IMongoClient>(() => new MongoClient(GetMongoConnectionString()));

        _redis = new RedisBuilder().WithPortBinding(6379, true).Build();
        _redisMultiplexer = new Lazy<IConnectionMultiplexer>(() =>
            ConnectionMultiplexer.Connect(GetRedisConnectionString())
        );
    }

    public async Task Startup()
    {
        await _mongoDb.StartAsync();
        await _redis.StartAsync();
    }

    public IMongoDatabase GetMongoDatabase()
    {
        return _mongoClient.Value.GetDatabase(GetMongoConnectionString().DatabaseName);
    }

    public IConnectionMultiplexer GetRedisMultiplexer()
    {
        return _redisMultiplexer.Value;
    }

    public async ValueTask DisposeAsync()
    {
        await _mongoDb.DisposeAsync();
        await _redis.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    public string GetRedisConnectionString()
    {
        return $"localhost:{_redis.GetMappedPublicPort(6379)}";
    }

    private MongoUrl GetMongoConnectionString()
    {
        return MongoUrl.Create(
            $"mongodb://{MONGO_USERNAME}:{MONGO_PASSWORD}@localhost:{_mongoDb.GetMappedPublicPort(27017)}/floppybot?authSource=admin&authMechanism=SCRAM-SHA-256"
        );
    }
}
