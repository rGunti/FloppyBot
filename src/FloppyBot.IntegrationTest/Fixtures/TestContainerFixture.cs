using DotNet.Testcontainers.Containers;
using MongoDB.Driver;
using StackExchange.Redis;
using Testcontainers.MongoDb;
using Testcontainers.Redis;

namespace FloppyBot.IntegrationTest.Fixtures;

[CollectionDefinition(NAME)]
public class TestContainerCollection : ICollectionFixture<TestContainerFixture>
{
    public const string NAME = "TestContainer";
}

public class TestContainerFixture : IAsyncDisposable
{
    private readonly Lazy<MongoClient> _mongoClient;
    private readonly IContainer _mongoDbContainer;
    private readonly Lazy<IConnectionMultiplexer> _redisConnection;
    private readonly IContainer _redisContainer;

    public TestContainerFixture()
    {
        _mongoDbContainer = new MongoDbBuilder().WithPortBinding(27017, true).Build();
        _redisContainer = new RedisBuilder().WithPortBinding(6379, true).Build();

        _mongoClient = new Lazy<MongoClient>(
            () => new MongoClient(MongoUrl.Create(GetMongoConnectionString()))
        );
        _redisConnection = new Lazy<IConnectionMultiplexer>(
            () => StackExchange.Redis.ConnectionMultiplexer.Connect(GetRedisConnectionString())
        );
    }

    public IConnectionMultiplexer ConnectionMultiplexer => _redisConnection.Value;
    public MongoClient MongoClient => _mongoClient.Value;

    public async ValueTask DisposeAsync()
    {
        await _mongoDbContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public async Task Startup()
    {
        await _mongoDbContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    public async Task Shutdown()
    {
        await _mongoDbContainer.StopAsync();
        await _redisContainer.StopAsync();
    }

    private string GetMongoConnectionString()
    {
        return $"mongodb://mongo:mongo@localhost:{_mongoDbContainer.GetMappedPublicPort(27017)}";
    }

    private string GetRedisConnectionString()
    {
        return $"localhost:{_redisContainer.GetMappedPublicPort(6379)}";
    }
}
