namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;

public interface IUnitConversion
{
    float Convert(float input);
    float ConvertBack(float input);
}
