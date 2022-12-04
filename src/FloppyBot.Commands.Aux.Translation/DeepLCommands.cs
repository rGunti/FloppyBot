using FloppyBot.Base.Extensions;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Translation.Commands;
using FloppyBot.Commands.Aux.Translation.Exceptions;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Aux.Translation;

[CommandHost]
public class DeepLCommands
{
    private const string CONFIG_KEY = "DeepL:ApiKey";

    public const string REPLY_HELP = "Translate anything using DeepL! You can either ask it in a sentence" +
                                     " (\"translate Hello World from English to German\") or a short syntax" +
                                     " (\"translate en>de Hello World\").";

    private const string REPLY_SUPPORTED_LANGUAGES =
        "The following languages are supported: {SupportedLanguages:list:{}|, |, and }";

    private const string REPLY_SUPPORTED_LANGUAGES_MD =
        "The following languages are supported: `{SupportedLanguages:list:{}|, |, and }`";

    private const string REPLY_SUPPORTED_LANGUAGE_CODES =
        "The following language codes are supported: {SupportedLanguageCodes:list:{}|, |, and }";

    private const string REPLY_SUPPORTED_LANGUAGE_CODES_MD =
        "The following language codes are supported: `{SupportedLanguageCodes:list:{}|, |, and }`";

    private const string REPLY_SUCCESS = "\"{Reply}\" (translated from {DetectedInputLanguage})";
    private const string REPLY_SUCCESS_MD = "> {Reply}\n_(translated from {DetectedInputLanguage})_";
    private const string REPLY_ERROR = "Whops! I couldn't translate this.";
    private const string REPLY_ERROR_SPECIFIC = "Whops! I couldn't translate this because {Message}.";

    private const string PARAM_LIST_LANGUAGES = "languages";
    private const string PARAM_LIST_LANGUAGE_CODES = "codes";

    private readonly ILogger<DeepLCommands> _logger;
    private readonly ITranslator _translator;

    public DeepLCommands(
        ILogger<DeepLCommands> logger,
        ITranslator translator)
    {
        _logger = logger;
        _translator = translator;
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void DiSetup(IServiceCollection services)
    {
        services
            .AddTransient<ITranslator, Translator>()
            .AddTransient<DeepL.ITranslator, DeepL.Translator>(p =>
                new DeepL.Translator(p.GetRequiredService<IConfiguration>()[CONFIG_KEY]));
    }

    [Command("translate")]
    [CommandCooldown(PrivilegeLevel.Viewer, 25000)]
    // ReSharper disable once UnusedMember.Global
    public CommandResult Translate(
        [AllArguments] string? inputString,
        [SupportsFeature(ChatInterfaceFeatures.MarkdownText)]
        bool supportsMarkdown)
    {
        if (string.IsNullOrWhiteSpace(inputString))
        {
            return CommandResult.SuccessWith(REPLY_HELP);
        }

        if (inputString is PARAM_LIST_LANGUAGES or PARAM_LIST_LANGUAGE_CODES)
        {
            var languages = new
            {
                SupportedLanguages = _translator.ListSupportedLanguages()
                    .Select(language => language.Capitalize())
                    .OrderBy(i => i),
                SupportedLanguageCodes = _translator.ListSupportedLanguageCodes()
                    .OrderBy(i => i),
            };
            string template = (inputString, supportsMarkdown) switch
            {
                (PARAM_LIST_LANGUAGES, true) => REPLY_SUPPORTED_LANGUAGES_MD,
                (PARAM_LIST_LANGUAGES, false) => REPLY_SUPPORTED_LANGUAGES,
                (PARAM_LIST_LANGUAGE_CODES, true) => REPLY_SUPPORTED_LANGUAGE_CODES_MD,
                (PARAM_LIST_LANGUAGE_CODES, false) => REPLY_SUPPORTED_LANGUAGE_CODES,
                _ => throw new ArgumentOutOfRangeException(
                    $"Combination of parameter {inputString} and markdown support {supportsMarkdown} is not implemented")
            };

            return CommandResult.SuccessWith(template.Format(languages));
        }

        try
        {
            string replyTemplate = supportsMarkdown ? REPLY_SUCCESS_MD : REPLY_SUCCESS;
            return _translator
                .ParseRequest(inputString)
                .Select(_translator.Translate)
                .Where(translation => !string.IsNullOrWhiteSpace(translation.Reply))
                .Select(translation => CommandResult.SuccessWith(replyTemplate.Format(translation)))
                .FirstOrDefault(CommandResult.FailedWith(REPLY_ERROR));
        }
        catch (TranslationException ex)
        {
            return CommandResult.FailedWith(REPLY_ERROR_SPECIFIC.Format(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to translate the following query: {TranslationQuery}", inputString);
            return CommandResult.FailedWith(REPLY_ERROR);
        }
    }
}

