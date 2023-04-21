using FloppyBot.Base.Testing;
using FloppyBot.Commands.Aux.Translation.Exceptions;
using FloppyBot.Commands.Core.Entities;
using Moq;

namespace FloppyBot.Commands.Aux.Translation.Tests;

[TestClass]
public class TranslatorCommandTests
{
    private readonly DeepLCommands _host;
    private readonly Mock<ITranslator> _translatorMock;

    public TranslatorCommandTests()
    {
        _translatorMock = new Mock<ITranslator>();
        _host = new DeepLCommands(LoggingUtils.GetLogger<DeepLCommands>(), _translatorMock.Object);

        _translatorMock
            .Setup(s => s.ListSupportedLanguages())
            .Returns(new[] { "swedish", "english", "german", "french" });
        _translatorMock
            .Setup(s => s.ListSupportedLanguageCodes())
            .Returns(new[] { "zh", "en", "de", "fr", "sv" });
    }

    [DataTestMethod]
    [DataRow(null, DisplayName = "null")]
    [DataRow("", DisplayName = "empty string")]
    [DataRow("     ", DisplayName = "empty string with whitespaces")]
    public void ReturnsHelpText(string? input)
    {
        Assert.AreEqual(
            CommandResult.SuccessWith(DeepLCommands.REPLY_HELP),
            _host.Translate(input, false)
        );
    }

    [TestMethod]
    public void ReturnsListOfLanguages()
    {
        Assert.AreEqual(
            CommandResult.SuccessWith(
                "The following languages are supported: English, French, German, and Swedish"
            ),
            _host.Translate("languages", false)
        );
        Assert.AreEqual(
            CommandResult.SuccessWith(
                "The following languages are supported: `English, French, German, and Swedish`"
            ),
            _host.Translate("languages", true)
        );
    }

    [TestMethod]
    public void ReturnsListOfLanguageCodes()
    {
        Assert.AreEqual(
            CommandResult.SuccessWith(
                "The following language codes are supported: de, en, fr, sv, and zh"
            ),
            _host.Translate("codes", false)
        );
        Assert.AreEqual(
            CommandResult.SuccessWith(
                "The following language codes are supported: `de, en, fr, sv, and zh`"
            ),
            _host.Translate("codes", true)
        );
    }

    [TestMethod]
    public void ReturnsTranslation()
    {
        _translatorMock
            .Setup(t => t.ParseRequest(It.IsAny<string>()))
            .Returns<string>(_ => new TranslationRequest("en", "de", "Hello World"));
        _translatorMock
            .Setup(t => t.Translate(It.IsAny<TranslationRequest>()))
            .Returns<TranslationRequest>(
                req => new TranslationResponse(req, "Hallo Welt", "German")
            );

        Assert.AreEqual(
            CommandResult.SuccessWith("\"Hallo Welt\" (translated from German)"),
            _host.Translate("Hello World from English to German", false)
        );
        Assert.AreEqual(
            CommandResult.SuccessWith("> Hallo Welt\n_(translated from German)_"),
            _host.Translate("Hello World from English to German", true)
        );
    }

    [TestMethod]
    public void ReturnsTranslationError()
    {
        _translatorMock
            .Setup(t => t.ParseRequest(It.IsAny<string>()))
            .Throws(new TranslationException("I didn't understand your query"));

        Assert.AreEqual(
            CommandResult.FailedWith(
                "Whops! I couldn't translate this because I didn't understand your query."
            ),
            _host.Translate("Hello World from English to German", false)
        );
    }

    [TestMethod]
    public void ReturnsErrorOnException()
    {
        _translatorMock
            .Setup(t => t.ParseRequest(It.IsAny<string>()))
            .Returns<string>(_ => new TranslationRequest("en", "de", "Hello World"));
        _translatorMock
            .Setup(t => t.Translate(It.IsAny<TranslationRequest>()))
            .Throws(new Exception("something happened"));

        Assert.AreEqual(
            CommandResult.FailedWith("Whops! I couldn't translate this."),
            _host.Translate("Hello World from English to German", false)
        );
    }
}
