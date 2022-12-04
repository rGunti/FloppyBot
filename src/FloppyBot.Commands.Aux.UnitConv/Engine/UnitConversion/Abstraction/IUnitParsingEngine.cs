using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.DTOs;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;

public interface IUnitParsingEngine
{
    IList<Unit> RegisteredUnits { get; }
    Unit DefaultUnit { get; }

    Unit GetUnit(string name);
    bool HasUnit(string name);
    bool TryGetUnit(string name, out Unit unit);

    UnitValue? ParseUnit(string input);
}

