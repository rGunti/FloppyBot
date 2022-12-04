using System.Collections.Immutable;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
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
    private const string REPLY_ERROR_PARSE_EMPTY = "Sorry but you didn't provide anything to convert.";
    private const string REPLY_ERROR_UNIT_EMPTY = "Sorry but you didn't provide a unit.";
    private const string REPLY_ERROR_UNITS_EMPTY = "Sorry but you didn't provide two units.";

    private const string REPLY_ERROR_CONVERSION =
        "Sorry but I don't know how to convert {Original.Unit.Symbol} to {DestinationUnit.Symbol}";

    private const string REPLY_DEBUG_UNITS =
        "The following units are known: {Units:list:{}|, } (Page {CurrentPage}/{MaxPage})";

    private const string REPLY_DEBUG_UNITS_MD =
        "The following units are known:\n{Units:list:- {}|\\n}\n_Page {CurrentPage} of {MaxPage}_";

    private const string REPLY_DEBUG_UNIT = "{Unit.DebugString}";

    private const string REPLY_DEBUG_UNIT_MD =
        "**About Unit `{Unit.Symbol}`**\nFull Name: **{Unit.FullName}**\nSymbol: `{Unit.Symbol}`\nParsing Expression: `{Unit.ParsingExpression}`";

    private const string REPLY_DEBUG_PARSE = "Parsed to {Value.DebugString}";
    private const string REPLY_DEBUG_PARSE_MD = "Parsed to _{Value}_\nDetected unit was _{Value.Unit.MdDebugString}_";

    private const string REPLY_DEBUG_CONVERT =
        "Can convert from {SourceUnit} to {DestinationUnit} using conversion {Conversion}";

    private const string REPLY_DEBUG_CONVERT_MD =
        "Can convert from **{SourceUnit}** to **{DestinationUnit}** using conversion `{Conversion}`";

    private const string REPLY_DEBUG_NO_CONVERT = "Cannot convert from {SourceUnit} to {DestinationUnit}";
    private const string REPLY_DEBUG_NO_CONVERT_MD = "Cannot convert from **{SourceUnit}** to **{DestinationUnit}**";

    private const int DEBUG_PAGE_SIZE = 10;

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
                    .OrderBy(u => u.FullName)
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

    [Command("unitdebug")]
    [CommandCategory("Diagnostics")]
    [MinCommandPrivilege(PrivilegeLevel.Moderator)]
    public CommandResult ShowUnitConversion(
        [ArgumentIndex(0, stopIfMissing: false)] string? subCommand,
        [ArgumentIndex(1, stopIfMissing: false)]
        string? subCommandParam1,
        [ArgumentIndex(2, stopIfMissing: false)]
        string? subCommandParam2,
        [SupportsFeature(ChatInterfaceFeatures.MarkdownText)]
        bool supportsMarkdown)
    {
        switch (subCommand)
        {
            case "unit":
                return PrintUnit(subCommandParam1, supportsMarkdown);
            case "units":
                ;
                return PrintUnitList(subCommandParam1, supportsMarkdown);
            case "convert":
                return PrintConversion(subCommandParam1, subCommandParam2, supportsMarkdown);
            case "parse":
                return PrintParse(subCommandParam1, supportsMarkdown);
            default:
                return CommandResult.FailedWith(
                    "Available debug commands are: unit <Unit>, units [<Page>], convert [<Source Unit> <Target Unit>], parse <Value>");
        }
    }

    private CommandResult PrintUnit(string? unitSymbol, bool supportsMarkdown)
    {
        if (string.IsNullOrWhiteSpace(unitSymbol))
        {
            return CommandResult.FailedWith(REPLY_ERROR_UNIT_EMPTY);
        }

        if (!_parsingEngine.HasUnit(unitSymbol))
        {
            return CommandResult.FailedWith(REPLY_ERROR_DESTINATION_UNIT);
        }

        return _parsingEngine.GetUnit(unitSymbol)
            .Wrap()
            .Select(unit => (supportsMarkdown ? REPLY_DEBUG_UNIT_MD : REPLY_DEBUG_UNIT)
                .Format(new { Unit = unit }))
            .Select(CommandResult.SuccessWith)
            .FirstOrDefault(CommandResult.FailedWith("failed"));
    }

    private CommandResult PrintUnitList(string? pageStr, bool supportsMarkdown)
    {
        IList<Unit> units = _parsingEngine.RegisteredUnits;
        int totalPages = (int)Math.Ceiling((decimal)_parsingEngine.RegisteredUnits.Count / DEBUG_PAGE_SIZE);
        int page = 0;
        if (pageStr != null && int.TryParse(pageStr, out page))
        {
            // Make user input more natural (start index 1 instead of 0)
            page -= 1;
        }

        page = Math.Max(0, Math.Min(page, totalPages - 1));

        return CommandResult.SuccessWith((supportsMarkdown ? REPLY_DEBUG_UNITS_MD : REPLY_DEBUG_UNITS).Format(
            new
            {
                Units = units
                    .OrderBy(u => u.FullName.ToLowerInvariant())
                    .Skip(page * DEBUG_PAGE_SIZE)
                    .Take(DEBUG_PAGE_SIZE)
                    .Select(u => supportsMarkdown ? u.MdDebugString : u.DebugString),
                CurrentPage = page + 1,
                MaxPage = totalPages
            }));
    }

    private CommandResult PrintConversion(string? sourceUnitStr, string? destinationUnitStr, bool supportsMarkdown)
    {
        if (string.IsNullOrWhiteSpace(sourceUnitStr) || string.IsNullOrWhiteSpace(destinationUnitStr))
        {
            return CommandResult.FailedWith(REPLY_ERROR_UNITS_EMPTY);
        }

        if (!_parsingEngine.TryGetUnit(sourceUnitStr, out Unit sourceUnit)
            || !_parsingEngine.TryGetUnit(destinationUnitStr, out Unit destinationUnit))
        {
            return CommandResult.FailedWith(REPLY_ERROR_DESTINATION_UNIT);
        }

        return _conversionEngine.FindConversion(sourceUnit, destinationUnit)
            .Wrap()
            .Select(conversion => (supportsMarkdown ? REPLY_DEBUG_CONVERT_MD : REPLY_DEBUG_CONVERT)
                .Format(new
                {
                    SourceUnit = sourceUnit,
                    DestinationUnit = destinationUnit,
                    Conversion = conversion
                }))
            .Select(CommandResult.SuccessWith)
            .FirstOrDefault(CommandResult.FailedWith((supportsMarkdown
                ? REPLY_DEBUG_NO_CONVERT_MD
                : REPLY_DEBUG_NO_CONVERT).Format(new
            {
                SourceUnit = sourceUnit,
                DestinationUnit = destinationUnit,
            })));
    }

    private CommandResult PrintParse(string? inputString, bool supportsMarkdown)
    {
        if (string.IsNullOrWhiteSpace(inputString))
        {
            return CommandResult.FailedWith(REPLY_ERROR_PARSE_EMPTY);
        }

        return _parsingEngine.ParseUnit(inputString)
            .Wrap()
            .Select(parsedValue => (supportsMarkdown ? REPLY_DEBUG_PARSE_MD : REPLY_DEBUG_PARSE).Format(new
            {
                Value = parsedValue
            }))
            .Select(CommandResult.SuccessWith)
            .FirstOrDefault(CommandResult.FailedWith(REPLY_ERROR_PARSE_SOURCE));
    }
}

