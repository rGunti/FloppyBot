using FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Abstraction;
using FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.DTOs;

namespace FloppyBot.Commands.Aux.Unit.Engine.UnitConversion;

public static class UnitConversionExtension
{
    public static float Convert(this IUnitConversion conversion, UnitValue value) => conversion.Convert(value.Value);

    public static UnitValue ConvertToUnit(this IUnitConversion conversion, UnitValue value, DTOs.Unit destinationUnit)
        => new UnitValue(conversion.Convert(value), destinationUnit);

    public static float ConvertBack(this IUnitConversion conversion, UnitValue value)
        => conversion.ConvertBack(value.Value);

    public static UnitValue ConvertBackToUnit(
        this IUnitConversion conversion,
        UnitValue value,
        DTOs.Unit destinationUnit)
        => new UnitValue(conversion.ConvertBack(value), destinationUnit);

    public static UnitValue As(this float value, DTOs.Unit unit) => new UnitValue(value, unit);

    public static IUnitConversion FindConversion(
        this IUnitConversionEngine engine,
        DTOs.Unit from,
        DTOs.Unit to)
        => engine.FindConversion(from.Symbol, to.Symbol);
}
