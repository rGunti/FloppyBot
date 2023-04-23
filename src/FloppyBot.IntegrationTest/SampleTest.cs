using FloppyBot.Base.Storage.MongoDb;
using FloppyBot.Communication.Redis;
using FloppyBot.FileStorage.Entities;
using FloppyBot.IntegrationTest.Fixtures;
using FluentAssertions;

namespace FloppyBot.IntegrationTest;

[Collection(TestContainerCollection.NAME)]
public class SampleTest : IAsyncLifetime, IAsyncDisposable
{
    private readonly TestContainerFixture _containers;
    private MongoDbRepositoryFactory _repositoryFactory = null!;

    public SampleTest(TestContainerFixture containerFixture)
    {
        _containers = containerFixture;
    }

    [Fact]
    public void TestMongoDatabase()
    {
        // arrange
        var repository = _repositoryFactory.GetRepository<FileHeader>();
        var fileHeader = new FileHeader("my-id", "Panda", "Sample File", 25000, "text/plain");

        // act
        repository.Insert(fileHeader);

        // assert
        var storedFileHeaders = repository.GetAll().ToArray();
        storedFileHeaders
            .Should()
            .HaveCount(1, "because we only inserted one document")
            .And.ContainInOrder(fileHeader);
    }

    [Fact]
    public void TestRedis()
    {
        // arrange
        var sender = new RedisNotificationSender(
            _containers.ConnectionMultiplexer.GetSubscriber(),
            "Sample.Channel"
        );
        var receiver = new RedisNotificationReceiver<FileHeader>(
            _containers.ConnectionMultiplexer.GetSubscriber(),
            "Sample.Channel"
        );

        var sampleHeader = new FileHeader("sampleHeader", "Panda", "Some Name", 240, "text/plain");
        using var monitor = receiver.Monitor();

        var pause = new ManualResetEvent(false);
        receiver.NotificationReceived += _ => pause.Set();
        receiver.StartListening();

        // act
        sender.Send(sampleHeader);
        pause.WaitOne(2500).Should().BeTrue();
        receiver.StopListening();

        // assert
        monitor
            .Should()
            .Raise(nameof(receiver.NotificationReceived))
            .WithArgs<FileHeader>(receivedHeader => receivedHeader == sampleHeader);
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

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await _containers.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
