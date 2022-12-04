using System.Text.RegularExpressions;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.DTOs;

public record Unit
{
    public Unit(
        string symbol,
        string fullName,
        string? regex,
        Func<string, MatchCollection, float> parsingFunction,
        Func<float, string>? formatMethod = null)
    {
        Symbol = symbol;
        FullName = fullName;
        ParsingExpression = regex != null ? new Regex(regex) : null;
        ParsingFunction = parsingFunction;
        FormatFunction = formatMethod ?? (v => $"{v:0.##} ???");
    }

    public string Symbol { get; }
    public string FullName { get; }
    public Regex? ParsingExpression { get; }
    public Func<string, MatchCollection, float> ParsingFunction { get; }
    public Func<float, string> FormatFunction { get; }

    public string DebugString => $"{this} (Expr: {ParsingExpression})";

    public override string ToString()
    {
        return $"Unit {FullName} [{Symbol}]";
    }
}

