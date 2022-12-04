using FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Abstraction;
using FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Implementation;

namespace FloppyBot.Commands.Aux.Unit.Engine.UnitConversion;

public static class UnitConvert
{
    public static IUnitParsingEngine DefaultParser = new UnitParsingEngine(
        Units.AllUnits, Units.DefaultUnit);

    public static IUnitConversionEngine DefaultConverter = new UnitConversionEngine(
        Units.AllConversions,
        Units.AllProxyConversions,
        DefaultParser.RegisteredUnits);
}
