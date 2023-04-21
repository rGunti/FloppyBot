using FloppyBot.Base.Extensions;

namespace FloppyBot.Commands.Aux.Translation.Commands;

public interface ITranslator
{
    NullableObject<TranslationRequest> ParseRequest(string inputString);
    TranslationResponse Translate(TranslationRequest request);
    string TranslateLanguageCodeToName(string languageCode);
    IEnumerable<string> ListSupportedLanguages();
    IEnumerable<string> ListSupportedLanguageCodes();
}
