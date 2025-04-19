using FloppyBot.Base.Storage.MongoDb;
using FloppyBot.Communication.Redis;
using FloppyBot.FileStorage.Entities;
using FloppyBot.IntTest.Fixtures;
using FloppyBot.IntTest.Utils;
using FluentAssertions.Extensions;
using Microsoft.Extensions.Logging;

namespace FloppyBot.IntTest;

[Collection(FloppyBotTestCollection.NAME)]
public class ServiceInterfaceTest : IAsyncDisposable
{
    private readonly FloppyBotTestContainerFixture _testContainerFixture;

    public ServiceInterfaceTest(FloppyBotTestContainerFixture testContainerFixture)
    {
        _testContainerFixture = testContainerFixture;
    }

    [Fact(
        Skip = "System.NullReferenceException: Object reference not set to an instance of an object"
    )]
    public async Task TestMongoDatabase()
    {
        await _testContainerFixture.Startup();

        var repositoryFactory = new MongoDbRepositoryFactory(
            _testContainerFixture.GetMongoDatabase(),
            A.Fake<ILogger<MongoDbRepositoryFactory>>()
        );

        // arrange
        var repository = repositoryFactory.GetRepository<FileHeader>();
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

    [Fact(
        Skip = "System.NullReferenceException: Object reference not set to an instance of an object"
    )]
    public async Task TestRedis()
    {
        await _testContainerFixture.Startup();

        var redisFactory = new RedisNotificationInterfaceFactory(
            A.Fake<ILogger<RedisNotificationInterfaceFactory>>(),
            new RedisConnectionFactory()
        );

        var baseConnectionString = _testContainerFixture.GetRedisConnectionString();
        var channelConnectionString = $"{baseConnectionString}|TestChannel";

        var fileHeaderToSend = new FileHeader("my-id", "Panda", "Sample File", 25000, "text/plain");

        var sender = redisFactory.GetNewSender(channelConnectionString);
        var receiver = redisFactory.GetNewReceiver<FileHeader>(channelConnectionString);
        using var receiverMonitor = receiver.Monitor();
        receiver.StartListening();

        await Wait.DoAndWaitUntil(
            a => receiver.NotificationReceived += _ => a.DeclareRaised(),
            () => sender.Send(fileHeaderToSend),
            5.Seconds()
        );
        receiverMonitor
            .Should()
            .Raise(nameof(receiver.NotificationReceived))
            .WithArgs<FileHeader>(header => header == fileHeaderToSend);

        receiver.StopListening();
    }

    public async ValueTask DisposeAsync()
    {
        await _testContainerFixture.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
