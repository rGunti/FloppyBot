using FloppyBot.Base.Clock;
using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Commands.Aux.Timer.Storage;
using FloppyBot.Commands.Aux.Timer.Storage.Entities;

namespace FloppyBot.Commands.Aux.Timer.Tests;

[TestClass]
public class TimerServiceTests
{
    private static readonly DateTimeOffset RefTime = DateTimeOffset.Parse("2022-12-12T12:00:00Z");
    private readonly IRepository<TimerRecord> _repository;
    private readonly LiteDbRepositoryFactory _repositoryFactory;

    private readonly TimerService _service;
    private readonly FixedTimeProvider _timeProvider;

    public TimerServiceTests()
    {
        _timeProvider = new FixedTimeProvider(RefTime);
        _repositoryFactory = LiteDbRepositoryFactory.CreateMemoryInstance();
        _repository = _repositoryFactory.GetRepository<TimerRecord>();
        _service = new TimerService(_repositoryFactory, _timeProvider);
    }

    [TestMethod]
    public void CreateNewRecord()
    {
        _service.CreateTimer("Mock/Channel/Message", "Mock/User", 5.Minutes(), "Hello World");

        Assert.AreEqual(
            new TimerRecord(
                "test",
                "Mock/Channel/Message",
                "Hello World",
                DateTimeOffset.Parse("2022-12-12T12:05:00Z"),
                RefTime,
                "Mock/User"
            ),
            _repository.GetAll().Select(t => t.WithId("test")).Single()
        );
    }

    [TestMethod]
    public void FetchExpiredTimers()
    {
        var expectedMessage = new TimerRecord(
            "test",
            "Mock/Channel/Message1",
            "Expected",
            DateTimeOffset.Parse("2022-12-12T12:05:00Z"),
            RefTime,
            "Mock/User"
        );
        var unexpectedMessage = new TimerRecord(
            "not_test",
            "Mock/Channel/Message2",
            "Unexpected",
            DateTimeOffset.Parse("2022-12-13T12:34:56Z"),
            RefTime,
            "Mock/AnotherUser"
        );
        _repository.Insert(expectedMessage);
        _repository.Insert(unexpectedMessage);

        _timeProvider.AdvanceTimeBy(5.Minutes());

        TimerRecord[] expiredMessages = _service.GetExpiredTimers(false).ToArray();

        CollectionAssert.AreEquivalent(new[] { expectedMessage }, expiredMessages);

        Assert.IsNotNull(_repository.GetById("test"), "Timer has been deleted unexpectedly");
    }

    [TestMethod]
    public void FetchAndDeleteExpiredTimers()
    {
        var expectedMessage = new TimerRecord(
            "test",
            "Mock/Channel/Message1",
            "Expected",
            DateTimeOffset.Parse("2022-12-12T12:05:00Z"),
            RefTime,
            "Mock/User"
        );
        var unexpectedMessage = new TimerRecord(
            "not_test",
            "Mock/Channel/Message2",
            "Unexpected",
            DateTimeOffset.Parse("2022-12-13T12:34:56Z"),
            RefTime,
            "Mock/AnotherUser"
        );
        _repository.Insert(expectedMessage);
        _repository.Insert(unexpectedMessage);

        _timeProvider.AdvanceTimeBy(5.Minutes());

        TimerRecord[] expiredMessages = _service.GetExpiredTimers(true).ToArray();

        CollectionAssert.AreEquivalent(new[] { expectedMessage }, expiredMessages);

        Assert.IsNull(_repository.GetById("test"), "Timer has not been deleted unexpectedly");
    }
}
