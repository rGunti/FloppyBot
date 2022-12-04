using System.Collections.Immutable;
using System.Reflection;
using System.Text.RegularExpressions;
using DeepL;
using DeepL.Model;
using FloppyBot.Base.Extensions;
using FloppyBot.Commands.Aux.Translation.Exceptions;

namespace FloppyBot.Commands.Aux.Translation.Commands;

public class Translator : ITranslator
{
    private const string LANGUAGE_CODE = "[a-z]{2}(-[A-Z]{2})?";

    private static readonly Regex LanguageCodeSelector = new(
        $"^{LANGUAGE_CODE}$",
        RegexOptions.Compiled);

    private static readonly Regex LanguageSelector = new(
        $"^({LANGUAGE_CODE})>({LANGUAGE_CODE})$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex NaturalTextToSelector = new(
        "^(.+) (to ([A-Za-z-]+))$",
        RegexOptions.Compiled);

    private static readonly Regex NaturalTextFromToSelector = new(
        "^(.+) (from ([A-Za-z-]+)) (to ([A-Za-z-]+))$",
        RegexOptions.Compiled);

    private static readonly ImmutableDictionary<string, string> LanguageCodeDictionary;
    private static readonly ImmutableDictionary<string, string> LanguageNameDictionary;
    private static readonly ImmutableDictionary<string, string> LanguageCodeToNameDictionary;

    private readonly DeepL.ITranslator _deepLTranslator;

    static Translator()
    {
        FieldInfo[] langFields = typeof(LanguageCode)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .ToArray();
        LanguageCodeDictionary = langFields
            .ToImmutableDictionary(
                f => ((string)f.GetRawConstantValue()!).ToLowerInvariant(),
                f => (string)f.GetRawConstantValue()!);
        LanguageNameDictionary = langFields
            .ToImmutableDictionary(
                f => f.Name.ToLowerInvariant(),
                f => (string)f.GetRawConstantValue()!);
        LanguageCodeToNameDictionary = langFields
            .ToImmutableDictionary(
                f => (string)f.GetRawConstantValue()!,
                f => f.Name);
    }

    public Translator(DeepL.ITranslator deepLTranslator)
    {
        _deepLTranslator = deepLTranslator;
    }

    public NullableObject<TranslationRequest> ParseRequest(string inputString)
    {
        if (string.IsNullOrWhiteSpace(inputString))
        {
            throw new ArgumentException("The input cannot be empty", nameof(inputString));
        }

        // Parameters
        string? inputLanguage = null;
        string? outputLanguage = null;
        string? inputText = null;

        // Test for language selector
        Match candidateMatch = LanguageSelector.Match(inputString[..inputString.IndexOf(' ')]);
        if (candidateMatch.Success)
        {
            inputLanguage = candidateMatch.Groups[1].Value;
            outputLanguage = candidateMatch.Groups[3].Value;
            inputText = inputString[(inputString.IndexOf(' ') + 1)..];
        }

        if (inputText == null)
        {
            if (NaturalTextFromToSelector.IsMatch(inputString))
            {
                Match match = NaturalTextFromToSelector.Match(inputString);
                inputLanguage = match.Groups[3].Value;
                outputLanguage = match.Groups[5].Value;
                inputText = match.Groups[1].Value;
            }
            else if (NaturalTextToSelector.IsMatch(inputString))
            {
                Match match = NaturalTextToSelector.Match(inputString);
                outputLanguage = match.Groups[3].Value;
                inputText = match.Groups[1].Value;
            }
        }

        if (outputLanguage == null || inputText == null)
        {
            return NullableObject.Empty<TranslationRequest>();
        }

        if (inputLanguage != null)
        {
            inputLanguage = ParseLanguageString(inputLanguage);

            if (!ValidateLanguageCode(inputLanguage))
            {
                return NullableObject.Empty<TranslationRequest>();
            }
        }

        outputLanguage = ParseLanguageString(outputLanguage);

        if (!ValidateLanguageCode(outputLanguage))
        {
            return NullableObject.Empty<TranslationRequest>();
        }

        if (inputLanguage != null)
        {
            inputLanguage = LanguageCode.RemoveRegionalVariant(inputLanguage);
        }

        if (outputLanguage == LanguageCode.English)
        {
            outputLanguage = LanguageCode.EnglishBritish;
        }

        return new TranslationRequest(inputLanguage, outputLanguage, inputText).Wrap();
    }

    public TranslationResponse Translate(TranslationRequest request)
    {
        TextResult translation = _deepLTranslator.TranslateTextAsync(
                request.Text,
                request.InputLanguage,
                request.OutputLanguage)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        return new TranslationResponse(
            request,
            translation.Text,
            TranslateLanguageCodeToName(translation.DetectedSourceLanguageCode));
    }

    public IEnumerable<string> ListSupportedLanguages()
    {
        return LanguageNameDictionary.Keys;
    }

    public IEnumerable<string> ListSupportedLanguageCodes()
    {
        return LanguageCodeDictionary.Keys;
    }

    public string TranslateLanguageCodeToName(string languageCode)
    {
        return LanguageCodeToNameDictionary.GetValueOrDefault(languageCode) ?? "Unknown";
    }

    private static bool ValidateLanguageCode(string language)
    {
        return LanguageCodeSelector.IsMatch(language);
    }

    private static string ParseLanguageString(string languageString)
    {
        if (LanguageCodeDictionary.TryGetValue(languageString.ToLowerInvariant(), out string? langCodeA))
        {
            return langCodeA;
        }

        if (LanguageNameDictionary.TryGetValue(languageString.ToLowerInvariant(), out var langCodeB))
        {
            return langCodeB;
        }

        throw new TranslationException($"\"{languageString}\" is not a known language");
    }
}
