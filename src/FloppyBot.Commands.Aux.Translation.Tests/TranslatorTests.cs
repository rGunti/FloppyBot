using Moq;

namespace FloppyBot.Commands.Aux.Translation.Tests;

[TestClass]
public class TranslatorTests
{
    private readonly Translator _translator;

    public TranslatorTests()
    {
        _translator = new Translator(
            new Mock<DeepL.ITranslator>().Object);
    }

    [DataTestMethod]
    [DataRow("en>de Hello World", "en", "de", "Hello World")]
    [DataRow("Hello World from en to de", "en", "de", "Hello World")]
    [DataRow("Hello World from english to german", "en", "de", "Hello World")]
    [DataRow("Hello World to german", null, "de", "Hello World")]
    public void CanParseInputString(
        string input,
        string expectedInputLanguage,
        string expectedOutputLanguage,
        string expectedText)
    {
        TranslationRequest request = _translator.ParseRequest(input);
        Assert.AreEqual(
            new TranslationRequest(
                expectedInputLanguage,
                expectedOutputLanguage,
                expectedText),
            request);
    }
}

