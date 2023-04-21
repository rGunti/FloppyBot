namespace FloppyBot.Commands.Aux.UnitConv.Tests.UnitConversion
{
    public abstract class BaseUnitConversionTest
    {
        protected static IUnitConversionEngine UnitConversionEngine => UnitConvert.DefaultConverter;
        protected static IUnitParsingEngine UnitParsingEngine => UnitConvert.DefaultParser;
    }
}
