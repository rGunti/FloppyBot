namespace FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.DTOs;

public class UnitValue
{
    public UnitValue()
    {
    }

    public UnitValue(float value, Unit unit)
    {
        Value = value;
        Unit = unit;
    }

    public float Value { get; set; }
    public Unit Unit { get; set; }

    public string DebugString => $"{this} ({Unit})";

    public override string ToString()
    {
        if (Unit != null)
            return Unit.FormatFunction(Value);
        return $"{Value} of unknown unit";
    }
}
