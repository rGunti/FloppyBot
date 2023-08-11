using FakeItEasy;
using ITranslator = DeepL.ITranslator;

namespace FloppyBot.Commands.Aux.Translation.Tests;

[TestClass]
public class TranslatorTests
{
    private readonly Translator _translator = new(A.Fake<ITranslator>());

    [DataTestMethod]
    [DataRow("en>de Hello World", "en", "de", "Hello World")]
    [DataRow("Hello World from en to de", "en", "de", "Hello World")]
    [DataRow("Hello World from english to german", "en", "de", "Hello World")]
    [DataRow("Hello World to german", null, "de", "Hello World")]
    public void CanParseInputString(
        string input,
        string expectedInputLanguage,
        string expectedOutputLanguage,
        string expectedText
    )
    {
        TranslationRequest request = _translator.ParseRequest(input);
        Assert.AreEqual(
            new TranslationRequest(expectedInputLanguage, expectedOutputLanguage, expectedText),
            request
        );
    }
}
