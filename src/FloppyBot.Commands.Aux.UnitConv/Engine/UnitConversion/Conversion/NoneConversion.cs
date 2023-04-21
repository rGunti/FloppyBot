using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Conversion;

internal class NoneConversion : IUnitConversion
{
    public float Convert(float input) => input;

    public float ConvertBack(float input) => input;

    public override string ToString() => "Same()";
}
