using FloppyBot.Base.Clock;
using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;
using Moq;

namespace FloppyBot.Commands.Aux.Quotes.Tests;

[TestClass]
public class QuoteServiceTests
{
    private readonly Mock<IQuoteChannelMappingService> _quoteChannelMappingServiceMock;
    private readonly IRepository<Quote> _quoteRepository;
    private readonly QuoteService _quoteService;
    private readonly FixedTimeProvider _timeProvider;

    public QuoteServiceTests()
    {
        var memoryDb = LiteDbRepositoryFactory.CreateMemoryInstance();
        _quoteRepository = memoryDb.GetRepository<Quote>();
        _timeProvider = new FixedTimeProvider(DateTimeOffset.Parse("2022-10-12T12:34:56Z"));
        _quoteChannelMappingServiceMock = new Mock<IQuoteChannelMappingService>();
        _quoteService = new QuoteService(
            memoryDb,
            _quoteChannelMappingServiceMock.Object,
            _timeProvider);
    }

    [TestMethod]
    public void AddQuote()
    {
        _quoteChannelMappingServiceMock
            .Setup(q => q.GetQuoteChannelMapping(
                It.Is<string>(s => s == "Mock/Channel"),
                It.Is<bool>(b => b == true)))
            .Returns<string, bool>((_, _) => "MappingId123");

        var newQuote = _quoteService.AddQuote(
            "Mock/Channel",
            "This is my quote",
            "Cool Game",
            "My User Name");

        Assert.AreEqual(
            new Quote(
                newQuote.Id,
                "MappingId123",
                1,
                "This is my quote",
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "My User Name"),
            newQuote);
    }
}
