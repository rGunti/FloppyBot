using FloppyBot.Base.Storage.MongoDb;
using FloppyBot.Communication.Redis;
using FloppyBot.FileStorage.Entities;
using FloppyBot.IntegrationTest.Setup;

namespace FloppyBot.IntegrationTest;

public class SampleTest : IAsyncLifetime
{
    private readonly TestContainerSetup _containers;
    private MongoDbRepositoryFactory _repositoryFactory = null!;

    public SampleTest()
    {
        _containers = new TestContainerSetup();
    }

    [Fact]
    public void TestMongoDatabase()
    {
        var repository = _repositoryFactory.GetRepository<FileHeader>();
        var fileHeader = new FileHeader(null!, "Panda", "Sample File", 25000, "text/plain");
        repository.Insert(fileHeader);

        var fileHeaders = repository.GetAll().ToArray();

        Assert.Single(fileHeaders, h => h == fileHeader with { Id = null!, });
    }

    [Fact]
    public void TestRedis()
    {
        var sender = new RedisNotificationSender(
            _containers.ConnectionMultiplexer.GetSubscriber(),
            "Sample.Channel"
        );
        var receiver = new RedisNotificationReceiver<FileHeader>(
            _containers.ConnectionMultiplexer.GetSubscriber(),
            "Sample.Channel"
        );

        var sampleHeader = new FileHeader("sampleHeader", "Panda", "Some Name", 240, "text/plain");

        var receivedHeaders = new List<FileHeader>();
        receiver.NotificationReceived += (header) =>
        {
            receivedHeaders.Add(header);
        };
        receiver.StartListening();

        sender.Send(sampleHeader);

        Thread.Sleep(2000);

        receiver.StopListening();

        Assert.Single(receivedHeaders);
        Assert.Equal(sampleHeader, receivedHeaders.First());
    }

    public async Task InitializeAsync()
    {
        await _containers.Startup();
        _repositoryFactory = new MongoDbRepositoryFactory(
            _containers.MongoClient.GetDatabase("test")
        );
    }

    public async Task DisposeAsync()
    {
        await _containers.Shutdown();
    }
}
