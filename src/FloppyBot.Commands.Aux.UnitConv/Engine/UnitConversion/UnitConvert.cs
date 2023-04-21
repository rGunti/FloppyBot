using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Implementation;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion;

public static class UnitConvert
{
    public static IUnitParsingEngine DefaultParser { get; } =
        new UnitParsingEngine(Units.AllUnits, Units.DefaultUnit);

    public static IUnitConversionEngine DefaultConverter { get; } =
        new UnitConversionEngine(
            Units.AllConversions,
            Units.AllProxyConversions,
            DefaultParser.RegisteredUnits
        );
}
