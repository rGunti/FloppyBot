using System.Text.RegularExpressions;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Executor.Agent.Cmds.Currency;

[CommandHost]
// ReSharper disable once UnusedType.Global
public class CurrencyCommands
{
    private const string REPLY = "{Input} are about {Output}";
    private const string REPLY_MD = "**{Input}** are about **{Output}**";

    private static readonly Regex CurrencyExtraction = new(
        "^((in|to) )?(([A-Za-z]){3})$",
        RegexOptions.Compiled);

    private readonly ICurrencyConverter _currencyConverter;

    public CurrencyCommands(ICurrencyConverter currencyConverter)
    {
        _currencyConverter = currencyConverter;
    }

    [Command("currency")]
    // ReSharper disable once UnusedMember.Global
    public async Task<string> ConvertCurrency(
        [ArgumentIndex(0)] decimal value,
        [ArgumentIndex(1)] string inputCurrency,
        [ArgumentRange(2)] string targetCurrencyStr,
        [SupportedFeatures] ChatInterfaceFeatures features)
    {
        var inputValue = new CurrencyValue(inputCurrency, value);
        var targetCurrency = CurrencyExtraction.Match(targetCurrencyStr).Groups[3].Value;

        var output = await _currencyConverter.Convert(inputValue, targetCurrency);
        return (features.Supports(ChatInterfaceFeatures.MarkdownText) ? REPLY_MD : REPLY).Format(new
        {
            Input = inputValue,
            Output = output,
        });
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void RegisterDependencies(IServiceCollection services)
    {
        services
            .AddSingleton<ICurrencyConverter, CurrencyConverter>();
    }
}
