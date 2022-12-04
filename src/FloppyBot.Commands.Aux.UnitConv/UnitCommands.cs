using System.Collections.Immutable;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.DTOs;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Aux.UnitConv;

[CommandHost]
[CommandCategory("Utilities")]
public class UnitCommands
{
    private const string PARAM_HELP = "help";
    //private const string PARAM_DEBUG = "debug";

    private const string REPLY_CONVERTED = "{Original} are about {Converted}";
    private const string REPLY_CONVERTED_MD = "_{Original}_ are about _{Converted}_";

    private const string REPLY_HELP =
        "Converts one unit to another. Try this command as an example: \"unit 10m in cm\". The following units are supported: {Units:list:{}|, }";

    private const string REPLY_HELP_MD =
        "Converts one unit to another. Try this command as an example: `unit 10m in cm`. The following units are supported: `{Units:list:{}|, }`";

    private const string REPLY_ERROR_PARSE_SOURCE = "Sorry but I didn't understand what you want to convert.";
    private const string REPLY_ERROR_DESTINATION_UNIT = "Sorry but I don't know the unit you want to convert to.";
    private const string REPLY_ERROR_SAME_UNIT = "Did you just...?";

    private const string REPLY_ERROR_CONVERSION =
        "Sorry but I don't know how to convert {Original.Unit.Symbol} to {DestinationUnit.Symbol}";

    private static readonly ImmutableArray<string> AllowedFillerWords = ImmutableArray.Create<string>()
        .Add("in")
        .Add("to");

    private readonly IUnitConversionEngine _conversionEngine;
    private readonly IUnitParsingEngine _parsingEngine;

    public UnitCommands(IUnitParsingEngine parsingEngine, IUnitConversionEngine conversionEngine)
    {
        _parsingEngine = parsingEngine;
        _conversionEngine = conversionEngine;
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void DiSetup(IServiceCollection services)
    {
        services
            .AddSingleton<IUnitParsingEngine>(_ => UnitConvert.DefaultParser)
            .AddSingleton<IUnitConversionEngine>(_ => UnitConvert.DefaultConverter);
    }

    [Command("unit")]
    public CommandResult ConvertUnit(
        [ArgumentIndex(0, stopIfMissing: false)] string? commandOrSourceValueInput,
        [ArgumentIndex(1, stopIfMissing: false)]
        string? fillerWordOrDestinationUnitInput,
        [ArgumentIndex(2, stopIfMissing: false)]
        string? destinationUnitInput,
        [SupportsFeature(ChatInterfaceFeatures.MarkdownText)]
        bool supportsMarkdown)
    {
        if (commandOrSourceValueInput is null or PARAM_HELP)
        {
            return CommandResult.SuccessWith((supportsMarkdown ? REPLY_HELP_MD : REPLY_HELP).Format(new
            {
                Units = _parsingEngine.RegisteredUnits
                    .Select(u => u.Symbol)
                    .OrderBy(i => i)
                    .ToArray()
            }));
        }

        string destinationUnit = fillerWordOrDestinationUnitInput.OrThrow(()
            => new ArgumentNullException(nameof(fillerWordOrDestinationUnitInput)));
        if (AllowedFillerWords.Contains(destinationUnit))
        {
            destinationUnit = destinationUnitInput ?? throw new ArgumentNullException(nameof(destinationUnitInput));
        }

        UnitValue? sourceValue = _parsingEngine.ParseUnit(commandOrSourceValueInput);
        if (sourceValue?.Unit == null)
        {
            return CommandResult.FailedWith(REPLY_ERROR_PARSE_SOURCE);
        }

        if (!_parsingEngine.TryGetUnit(destinationUnit, out Unit parsedUnit))
        {
            return CommandResult.FailedWith(REPLY_ERROR_DESTINATION_UNIT);
        }

        if (sourceValue.Unit == parsedUnit)
        {
            return CommandResult.FailedWith(REPLY_ERROR_SAME_UNIT);
        }

        return _conversionEngine.FindConversion(sourceValue.Unit, parsedUnit)
            .Wrap()
            .Select(conversion => conversion.ConvertToUnit(sourceValue, parsedUnit))
            .Select(convertedValue => CommandResult.SuccessWith(
                (supportsMarkdown ? REPLY_CONVERTED_MD : REPLY_CONVERTED).Format(new
                {
                    Original = sourceValue,
                    Converted = convertedValue
                })))
            .FirstOrDefault(CommandResult.FailedWith(REPLY_ERROR_CONVERSION.Format(new
            {
                Original = sourceValue,
                DestinationUnit = parsedUnit
            })));
    }
}

