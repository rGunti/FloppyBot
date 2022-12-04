using FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Abstraction;

namespace FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Conversion;

internal class NoneConversion : IUnitConversion
{
    public float Convert(float input) => input;
    public float ConvertBack(float input) => input;

    public override string ToString() => "NoConversion()";
}
