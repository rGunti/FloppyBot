using System.Text.RegularExpressions;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Executor.Agent.Cmds.Currency;

[CommandHost]
[CommandCategory("Utilities")]
// ReSharper disable once UnusedType.Global
public class CurrencyCommands
{
    private const string REPLY = "{Input} are about {Output}";
    private const string REPLY_MD = "**{Input}** are about **{Output}**";

    private static readonly Regex CurrencyExtraction =
        new("^((in|to) )?(([A-Za-z]){3})$", RegexOptions.Compiled);

    private readonly ICurrencyConverter _currencyConverter;

    public CurrencyCommands(ICurrencyConverter currencyConverter)
    {
        _currencyConverter = currencyConverter;
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void RegisterDependencies(IServiceCollection services)
    {
        services.AddSingleton<ICurrencyConverter, CurrencyConverter>();
    }

    [Command("money", "currency")]
    [PrimaryCommandName("money")]
    [CommandDescription(
        "Converts the given amount of money into another currency. "
            + "The international three letter currency codes are to be provided."
    )]
    [CommandSyntax(
        "<Input> <Currency> [in|to] <Target Currency>",
        "20 EUR in USD",
        "15.90 CHF to NOK"
    )]
    // ReSharper disable once UnusedMember.Global
    public async Task<string> ConvertCurrency(
        [ArgumentIndex(0)] decimal value,
        [ArgumentIndex(1)] string inputCurrency,
        [ArgumentRange(2)] string targetCurrencyStr,
        [SupportedFeatures] ChatInterfaceFeatures features
    )
    {
        var inputValue = new CurrencyValue(inputCurrency, value);
        var targetCurrency = CurrencyExtraction.Match(targetCurrencyStr).Groups[3].Value;

        var output = await _currencyConverter.Convert(inputValue, targetCurrency);
        return (features.Supports(ChatInterfaceFeatures.MarkdownText) ? REPLY_MD : REPLY).Format(
            new { Input = inputValue, Output = output, }
        );
    }
}
