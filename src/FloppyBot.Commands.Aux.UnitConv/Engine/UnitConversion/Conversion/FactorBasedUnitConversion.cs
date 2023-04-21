using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Conversion;

internal class FactorBasedUnitConversion : IUnitConversion
{
    public FactorBasedUnitConversion(float factor)
    {
        Factor = factor;
    }

    public float Factor { get; }

    public float Convert(float input)
    {
        return input * Factor;
    }

    public float ConvertBack(float input)
    {
        return input / Factor;
    }

    public override string ToString() => $"Factor({Factor})";
}
