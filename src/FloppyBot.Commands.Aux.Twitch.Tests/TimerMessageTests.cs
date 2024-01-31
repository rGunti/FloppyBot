using FakeItEasy;
using FloppyBot.Aux.MessageCounter.Core;
using FloppyBot.Aux.MessageCounter.Core.Entities;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Base.Testing;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Twitch.Storage;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;
using FloppyBot.Commands.Core.Replier;

namespace FloppyBot.Commands.Aux.Twitch.Tests;

[TestClass]
public class TimerMessageTests
{
    private static readonly DateTimeOffset RefTime = DateTimeOffset.Parse("2022-12-12T12:00:00Z");

    private static readonly TimerMessageConfiguration RefConfig =
        new("Mock/Channel", new[] { "Hello World", "This is a test" }, 5, 5);

    private readonly IRepository<TimerMessageConfiguration> _configRepo;

    private readonly TimerMessageCronJob _cronJob;
    private readonly IRepository<TimerMessageExecution> _executionRepo;
    private readonly TimerMessageConfigurationService _messageConfigService;
    private readonly MessageOccurrenceService _messageOccurrenceService;
    private readonly IMessageReplier _messageReplier;

    private readonly IRepository<MessageOccurrence> _occurrenceRepo;

    private readonly List<ChatMessage> _sentMessages = new();
    private readonly FixedTimeProvider _timeProvider;

    public TimerMessageTests()
    {
        var db = LiteDbRepositoryFactory.CreateMemoryInstance();

        _timeProvider = new FixedTimeProvider(RefTime);
        _messageReplier = A.Fake<IMessageReplier>();
        A.CallTo(() => _messageReplier.SendMessage(A<ChatMessage>.Ignored))
            .Invokes((ChatMessage chatMessage) => _sentMessages.Add(chatMessage));
        _messageOccurrenceService = new MessageOccurrenceService(db, _timeProvider);
        _messageConfigService = new TimerMessageConfigurationService(db);

        _occurrenceRepo = db.GetRepository<MessageOccurrence>();
        _configRepo = db.GetRepository<TimerMessageConfiguration>();
        _executionRepo = db.GetRepository<TimerMessageExecution>();

        _cronJob = new TimerMessageCronJob(
            LoggingUtils.GetLogger<TimerMessageCronJob>(),
            _messageOccurrenceService,
            _messageReplier,
            _messageConfigService,
            _timeProvider
        );
    }

    [TestMethod]
    public void SendNoMessageWhenNothingIsAvailable()
    {
        _cronJob.Run();
        CollectionAssert.AreEqual(Array.Empty<ChatMessage>(), _sentMessages.ToArray());
        CollectionAssert.AreEqual(
            Array.Empty<TimerMessageExecution>(),
            _executionRepo.GetAll().ToArray()
        );
    }

    [TestMethod]
    public void SendNoMessageWhenNotMessageOcc()
    {
        _configRepo.Insert(RefConfig);
        _cronJob.Run();
        CollectionAssert.AreEqual(Array.Empty<ChatMessage>(), _sentMessages.ToArray());
        CollectionAssert.AreEqual(
            Array.Empty<TimerMessageExecution>(),
            _executionRepo.GetAll().ToArray()
        );
    }

    [TestMethod]
    public void SendNoMessageWhenNotEnoughMessageOcc()
    {
        GenerateMessageOccurrences(5, 120.Second());
        _configRepo.Insert(RefConfig);
        _cronJob.Run();

        CollectionAssert.AreEqual(Array.Empty<ChatMessage>(), _sentMessages.ToArray());
        CollectionAssert.AreEqual(
            Array.Empty<TimerMessageExecution>(),
            _executionRepo.GetAll().ToArray()
        );
    }

    [TestMethod]
    public void SendExpectedChatMessage()
    {
        GenerateMessageOccurrences(10, 30.Second());
        _configRepo.Insert(RefConfig);

        _cronJob.Run();

        CollectionAssert.AreEquivalent(
            new[]
            {
                new ChatMessage(
                    ChatMessageIdentifier.NewFor("Mock/Channel"),
                    ChatUser.Anonymous,
                    SharedEventTypes.CHAT_MESSAGE,
                    RefConfig.Messages[0]
                ),
            },
            _sentMessages.ToArray()
        );
        Assert.AreEqual(
            new TimerMessageExecution("Mock/Channel", RefTime, 0),
            _executionRepo.GetAll().Single()
        );

        _timeProvider.AdvanceTimeBy(10.Minutes());
        _sentMessages.Clear();
        GenerateMessageOccurrences(10, 30.Second(), 10);

        _cronJob.Run();

        CollectionAssert.AreEquivalent(
            new[]
            {
                new ChatMessage(
                    ChatMessageIdentifier.NewFor("Mock/Channel"),
                    ChatUser.Anonymous,
                    SharedEventTypes.CHAT_MESSAGE,
                    RefConfig.Messages[1]
                ),
            },
            _sentMessages.ToArray()
        );
        Assert.AreEqual(
            new TimerMessageExecution("Mock/Channel", RefTime + 10.Minutes(), 1),
            _executionRepo.GetAll().Single()
        );
    }

    private void GenerateMessageOccurrences(int amount, TimeSpan gap, int startId = 0)
    {
        var _ = Enumerable
            .Range(startId, amount)
            .Select(i => new MessageOccurrence(
                $"Mock/Channel/Message{i}",
                "Mock/Channel",
                "Mock/User",
                _timeProvider.GetCurrentUtcTime() - ((i - startId) * gap)
            ))
            .Select(_occurrenceRepo.Insert)
            .ToArray();
    }
}
