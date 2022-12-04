using System.Collections.Immutable;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.DTOs;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Implementation;

internal class UnitParsingEngine : IUnitParsingEngine
{
    private readonly IImmutableDictionary<string, Unit> _units;

    public UnitParsingEngine(IEnumerable<Unit> units, Unit defaultUnit)
    {
        _units = units.ToImmutableDictionary(u => u.Symbol, u => u);
        DefaultUnit = defaultUnit;
    }

    public IList<Unit> RegisteredUnits => _units.Values.ToImmutableList();

    public Unit DefaultUnit { get; }

    public Unit GetUnit(string name) => _units[name];
    public bool HasUnit(string name) => _units.ContainsKey(name);

    public bool TryGetUnit(string name, out Unit unit)
    {
        unit = null;
        if (HasUnit(name))
        {
            unit = GetUnit(name);
            return true;
        }

        return false;
    }

    public UnitValue? ParseUnit(string input)
    {
        foreach (var (_, unit) in _units)
        {
            var expr = unit.ParsingExpression;
            if (expr.IsMatch(input))
            {
                return new UnitValue(
                    unit.ParsingFunction(input, expr.Matches(input)),
                    unit);
            }
        }

        return null;
    }
}

