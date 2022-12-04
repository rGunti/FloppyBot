namespace FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Abstraction;

public interface IUnitConversion
{
    float Convert(float input);
    float ConvertBack(float input);
}
