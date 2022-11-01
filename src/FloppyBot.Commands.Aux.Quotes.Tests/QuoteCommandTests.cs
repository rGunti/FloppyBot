using FloppyBot.Base.Testing;
using FloppyBot.Commands.Aux.Quotes.Storage;
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
}
