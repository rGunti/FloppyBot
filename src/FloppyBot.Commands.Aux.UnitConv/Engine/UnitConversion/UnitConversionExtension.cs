using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.DTOs;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion;

public static class UnitConversionExtension
{
    public static float Convert(this IUnitConversion conversion, UnitValue value) => conversion.Convert(value.Value);

    public static UnitValue ConvertToUnit(this IUnitConversion conversion, UnitValue value, Unit destinationUnit)
        => new(conversion.Convert(value), destinationUnit);

    public static float ConvertBack(this IUnitConversion conversion, UnitValue value)
        => conversion.ConvertBack(value.Value);

    public static UnitValue ConvertBackToUnit(this IUnitConversion conversion, UnitValue value, Unit destinationUnit)
        => new(conversion.ConvertBack(value), destinationUnit);

    public static UnitValue As(this float value, Unit unit) => new(value, unit);

    public static IUnitConversion? FindConversion(
        this IUnitConversionEngine engine,
        Unit from,
        Unit to)
        => engine.FindConversion(from.Symbol, to.Symbol);
}

