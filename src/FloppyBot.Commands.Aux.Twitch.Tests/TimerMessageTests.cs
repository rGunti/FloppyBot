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
using Moq;

namespace FloppyBot.Commands.Aux.Twitch.Tests;

[TestClass]
public class TimerMessageTests
{
    private static readonly DateTimeOffset RefTime = DateTimeOffset.Parse("2022-12-12T12:00:00Z");
    private readonly IRepository<TimerMessageConfiguration> _configRepo;

    private readonly TimerMessageCronJob _cronJob;
    private readonly IRepository<TimerMessageExecution> _executionRepo;
    private readonly TimerMessageConfigurationService _messageConfigService;
    private readonly MessageOccurrenceService _messageOccurrenceService;
    private readonly Mock<IMessageReplier> _messageReplierMock;

    private readonly IRepository<MessageOccurrence> _occurrenceRepo;

    private readonly List<ChatMessage> _sentMessages = new();
    private readonly FixedTimeProvider _timeProvider;

    public TimerMessageTests()
    {
        var db = LiteDbRepositoryFactory.CreateMemoryInstance();

        _timeProvider = new FixedTimeProvider(RefTime);
        _messageReplierMock = new Mock<IMessageReplier>();
        _messageReplierMock
            .Setup(s => s.SendMessage(It.IsAny<ChatMessage>()))
            .Callback<ChatMessage>(chatMessage => _sentMessages.Add(chatMessage));
        _messageOccurrenceService = new MessageOccurrenceService(
            db,
            _timeProvider);
        _messageConfigService = new TimerMessageConfigurationService(db);

        _occurrenceRepo = db.GetRepository<MessageOccurrence>();
        _configRepo = db.GetRepository<TimerMessageConfiguration>();
        _executionRepo = db.GetRepository<TimerMessageExecution>();

        _cronJob = new TimerMessageCronJob(
            LoggingUtils.GetLogger<TimerMessageCronJob>(),
            _messageOccurrenceService,
            _messageReplierMock.Object,
            _messageConfigService,
            _timeProvider);
    }

    private void GenerateMessageOccurrences(int amount, TimeSpan gap)
    {
        var _ = Enumerable.Range(0, amount)
            .Select(i => new MessageOccurrence(
                $"Mock/Channel/Message{i}",
                "Mock/Channel",
                "Mock/User",
                RefTime - i * gap))
            .Select(_occurrenceRepo.Insert)
            .ToArray();
    }

    [TestMethod]
    public void SendNoMessageWhenNothingIsAvailable()
    {
        _cronJob.Run();
        CollectionAssert.AreEqual(
            Array.Empty<ChatMessage>(),
            _sentMessages.ToArray());
    }

    [TestMethod]
    public void SendExpectedChatMessage()
    {
        GenerateMessageOccurrences(10, 30.Second());
        _configRepo.Insert(new TimerMessageConfiguration(
            "Mock/Channel",
            new[]
            {
                "Hello World",
                "This is a test"
            },
            5,
            5));

        _cronJob.Run();
        CollectionAssert.AreEquivalent(
            new[]
            {
                new ChatMessage(
                    ChatMessageIdentifier.NewFor("Mock/Channel"),
                    ChatUser.Anonymous,
                    SharedEventTypes.CHAT_MESSAGE,
                    "Hello World")
            },
            _sentMessages.ToArray());
    }
}

