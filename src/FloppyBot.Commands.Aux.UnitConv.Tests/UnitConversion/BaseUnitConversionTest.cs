namespace FloppyBot.Commands.Aux.UnitConv.Tests.UnitConversion
{
    public abstract class BaseUnitConversionTest
    {
        protected readonly IUnitConversionEngine _unitConversionEngine = UnitConvert.DefaultConverter;
        protected readonly IUnitParsingEngine _unitParsingEngine = UnitConvert.DefaultParser;
    }
}
