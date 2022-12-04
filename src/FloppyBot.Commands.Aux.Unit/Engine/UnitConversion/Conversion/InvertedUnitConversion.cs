using FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Abstraction;

namespace FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Conversion;

internal class InvertedUnitConversion : IUnitConversion
{
    private readonly IUnitConversion _baseConversion;

    public InvertedUnitConversion(IUnitConversion baseConversion)
    {
        _baseConversion = baseConversion;
    }

    public float Convert(float input) => _baseConversion.ConvertBack(input);
    public float ConvertBack(float input) => _baseConversion.Convert(input);

    public override string ToString() => $"Invert({_baseConversion})";
}
