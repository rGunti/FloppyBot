using FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.DTOs;

namespace FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Abstraction;

public interface IUnitParsingEngine
{
    IList<DTOs.Unit> RegisteredUnits { get; }
    DTOs.Unit DefaultUnit { get; }

    DTOs.Unit GetUnit(string name);
    bool HasUnit(string name);
    bool TryGetUnit(string name, out DTOs.Unit unit);

    UnitValue ParseUnit(string input);
}
