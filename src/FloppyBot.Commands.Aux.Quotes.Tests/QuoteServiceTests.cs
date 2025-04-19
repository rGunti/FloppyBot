using FakeItEasy;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Rng;
using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.Commands.Aux.Quotes.Tests;

public class QuoteServiceTests
{
    private const string CHANNEL_ID = "Mock/Channel";
    private const string MAPPING_ID = "MappingId123";
    private readonly IQuoteChannelMappingService _quoteChannelMappingService;
    private readonly IRepository<Quote> _quoteRepository;
    private readonly QuoteService _quoteService;
    private readonly StaticNumberGenerator _rng;
    private readonly FixedTimeProvider _timeProvider;

    public QuoteServiceTests()
    {
        var memoryDb = LiteDbRepositoryFactory.CreateMemoryInstance();
        _quoteRepository = memoryDb.GetRepository<Quote>();
        _timeProvider = new FixedTimeProvider(DateTimeOffset.Parse("2022-10-12T12:34:56Z"));
        _quoteChannelMappingService = A.Fake<IQuoteChannelMappingService>();
        _rng = new StaticNumberGenerator(1);
        _quoteService = new QuoteService(
            memoryDb,
            _quoteChannelMappingService,
            _timeProvider,
            _rng
        );

        A.CallTo(() => _quoteChannelMappingService.GetQuoteChannelMapping(CHANNEL_ID, A<bool>._))
            .Returns(MAPPING_ID);
    }

    [Fact]
    public void AddQuote()
    {
        var newQuote = _quoteService.AddQuote(
            CHANNEL_ID,
            "This is my quote",
            "Cool Game",
            "My User Name"
        );

        Assert.Equal(
            new Quote(
                newQuote.Id,
                MAPPING_ID,
                1,
                "This is my quote",
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "My User Name"
            ),
            newQuote
        );
        Assert.Contains(
            _quoteRepository.GetAll(),
            i => i.ChannelMappingId == MAPPING_ID && i.QuoteId == 1
        );

        var newQuote2 = _quoteService.AddQuote(
            CHANNEL_ID,
            "This is my #2 quote",
            "Cool Game",
            "Other User Name"
        );
        Assert.Equal(
            new Quote(
                newQuote2.Id,
                MAPPING_ID,
                2,
                "This is my #2 quote",
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "Other User Name"
            ),
            newQuote2
        );
        Assert.Equivalent(
            new[] { 1, 2 },
            _quoteRepository
                .GetAll()
                .Where(i => i.ChannelMappingId == MAPPING_ID)
                .Select(q => q.QuoteId)
                .ToArray()
        );
    }

    [Fact]
    public void EditQuote()
    {
        _quoteRepository.Insert(
            new Quote(
                "myId",
                MAPPING_ID,
                1337,
                "My quote text",
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "My User Name"
            )
        );

        var editQuote = _quoteService.EditQuote(CHANNEL_ID, 1337, "New quote text");

        Assert.NotNull(editQuote);
        Assert.Equal(
            new Quote(
                "myId",
                MAPPING_ID,
                1337,
                "New quote text",
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "My User Name"
            ),
            editQuote
        );

        Assert.Null(_quoteService.EditQuote("Mock/OtherChannel", 1337, "New Content"));
    }

    [Fact]
    public void DeleteQuote()
    {
        _quoteRepository.Insert(
            new Quote(
                "myId",
                MAPPING_ID,
                1337,
                "My quote text",
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "My User Name"
            )
        );

        Assert.True(_quoteService.DeleteQuote(CHANNEL_ID, 1337));
        Assert.False(_quoteService.DeleteQuote(CHANNEL_ID, 1337));
    }

    [Fact]
    public void EditQuoteContext()
    {
        _quoteRepository.Insert(
            new Quote(
                "myId",
                MAPPING_ID,
                1337,
                "My quote text",
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "My User Name"
            )
        );

        var editQuote = _quoteService.EditQuoteContext(CHANNEL_ID, 1337, "New Game");

        Assert.Equal<Quote>(
            new Quote(
                "myId",
                MAPPING_ID,
                1337,
                "My quote text",
                "New Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "My User Name"
            ),
            editQuote
        );
        Assert.Contains(
            _quoteRepository.GetAll(),
            q =>
                q.ChannelMappingId == MAPPING_ID
                && q.QuoteId == 1337
                && q.QuoteContext == "New Game"
        );

        Assert.Null(_quoteService.EditQuoteContext("Mock/OtherChannel", 1337, "New Game"));
    }

    [Fact]
    public void GetQuote()
    {
        _quoteRepository.Insert(
            new Quote(
                "myId",
                MAPPING_ID,
                1337,
                "My quote text",
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "My User Name"
            )
        );

        Assert.Equal<Quote>(
            new Quote(
                "myId",
                MAPPING_ID,
                1337,
                "My quote text",
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "My User Name"
            ),
            _quoteService.GetQuote(CHANNEL_ID, 1337)
        );
        Assert.Null(_quoteService.GetQuote(CHANNEL_ID, 1338));
        Assert.Null(_quoteService.GetQuote("Mock/OtherChannel", 1337));
    }

    [Fact]
    public void GetRandomQuote()
    {
        Assert.Null(_quoteService.GetRandomQuote(CHANNEL_ID));

        _quoteRepository.Insert(
            new Quote(
                "myId",
                MAPPING_ID,
                1,
                "My quote text",
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "My User Name"
            )
        );

        var quote = _quoteService.GetRandomQuote(CHANNEL_ID);
        Assert.Equal<Quote>(
            new Quote(
                "myId",
                MAPPING_ID,
                1,
                "My quote text",
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "My User Name"
            ),
            quote
        );

        Assert.Null(_quoteService.GetRandomQuote("Mock/SomeOtherChannel"));

        // Test with changed rng
        _rng.SetNumbers(2);
        Assert.Null(_quoteService.GetRandomQuote(CHANNEL_ID));
    }
}
