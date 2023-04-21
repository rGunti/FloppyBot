namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.DTOs;

public record UnitValue(float Value, Unit? Unit)
{
    public string DebugString => $"{this} ({Unit})";

    public override string ToString()
    {
        if (Unit != null)
        {
            return Unit.FormatFunction(Value);
        }

        return $"{Value} of unknown unit";
    }
}
