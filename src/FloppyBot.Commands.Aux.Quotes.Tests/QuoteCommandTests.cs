using FloppyBot.Base.Testing;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;
using Moq;

namespace FloppyBot.Commands.Aux.Quotes.Tests;

[TestClass]
public class QuoteCommandTests
{
    private readonly QuoteCommands _quoteCommands;
    private readonly Mock<IQuoteService> _quoteServiceMock;

    public QuoteCommandTests()
    {
        _quoteServiceMock = new Mock<IQuoteService>();
        _quoteCommands = new QuoteCommands(
            LoggingUtils.GetLogger<QuoteCommands>(),
            _quoteServiceMock.Object);
    }

    [TestMethod]
    public void AddQuote()
    {
        _quoteServiceMock
            .Setup(q => q.AddQuote(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>()))
            .Returns((string _, string quoteText, string context, string author) => new Quote(
                "someId",
                "someChannelMapping",
                1337,
                quoteText,
                context,
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                author));

        var reply = _quoteCommands.AddQuote(
            "Mock/Channel",
            "Cool Game",
            new ChatUser(
                "Mock/UserName",
                "User Name",
                PrivilegeLevel.Viewer),
            "This is my quote");
        Assert.AreEqual(
            "Created new Quote #1337: This is my quote [Cool Game @ 2022-10-12]",
            reply);

        _quoteServiceMock
            .Verify(q => q.AddQuote(
                    It.Is<string>(s => s == "Mock/Channel"),
                    It.Is<string>(s => s == "This is my quote"),
                    It.Is<string>(s => s == "Cool Game"),
                    It.Is<string>(s => s == "User Name")),
                Times.Once);
    }

    [TestMethod]
    public void EditQuote()
    {
        _quoteServiceMock
            .Setup(q => q.EditQuote(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>()))
            .Returns<string, int, string>((channelId, quoteId, newContent) => new Quote(
                "someId",
                "someMappingId",
                quoteId,
                newContent,
                "Cool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "User Name"));

        var reply = _quoteCommands.EditQuote(
            "Mock/Channel",
            1337,
            "This is my new text");

        Assert.AreEqual(
            "Updated Quote #1337: This is my new text [Cool Game @ 2022-10-12]",
            reply);
        _quoteServiceMock
            .Verify(q => q.EditQuote(
                    It.Is<string>(s => s == "Mock/Channel"),
                    It.Is<int>(i => i == 1337),
                    It.Is<string>(s => s == "This is my new text")),
                Times.Once);
    }

    [TestMethod]
    public void EditQuoteContext()
    {
        _quoteServiceMock
            .Setup(q => q.EditQuoteContext(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>()))
            .Returns<string, int, string>((channelId, quoteId, newContext) => new Quote(
                "someId",
                "someMappingId",
                quoteId,
                "This is my quote",
                "Uncool Game",
                DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                "User Name"));

        var reply = _quoteCommands.EditQuoteContext(
            "Mock/Channel",
            1337,
            "Uncool Game");

        Assert.AreEqual(
            "Updated Quote #1337: This is my quote [Uncool Game @ 2022-10-12]",
            reply);
        _quoteServiceMock
            .Verify(q => q.EditQuoteContext(
                    It.Is<string>(s => s == "Mock/Channel"),
                    It.Is<int>(i => i == 1337),
                    It.Is<string>(s => s == "Uncool Game")),
                Times.Once);
    }

    [TestMethod]
    public void DeleteQuote()
    {
        _quoteServiceMock
            .Setup(q => q.DeleteQuote(
                It.IsAny<string>(),
                It.IsAny<int>()))
            .Returns(true);

        var reply = _quoteCommands.DeleteQuote(
            "Mock/Channel",
            1337);
        Assert.AreEqual(
            "Deleted Quote #1337",
            reply);

        _quoteServiceMock
            .Verify(q => q.DeleteQuote(
                    It.Is<string>(s => s == "Mock/Channel"),
                    It.Is<int>(i => i == 1337)),
                Times.Once);
    }
}
