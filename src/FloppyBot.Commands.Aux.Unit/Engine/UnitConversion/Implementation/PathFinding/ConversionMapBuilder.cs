using System.Collections.Immutable;
using FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Abstraction;

namespace FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Implementation.PathFinding;

public static class ConversionMapBuilder
{
    public static ConversionMap BuildMap(
        IEnumerable<DTOs.Unit> knownUnits,
        IImmutableDictionary<(string from, string to), IUnitConversion> knownConversions)
    {
        return BuildMap(
            knownUnits.Select(u => u.Symbol),
            knownConversions.Keys.ToHashSet());
    }

    public static ConversionMap BuildMap(
        IEnumerable<string> knownNodes,
        ISet<(string a, string b)> knownRelations)
    {
        var nodes = knownNodes.Distinct()
            .ToDictionary(u => u, u => new ConversionNode(u));

        foreach (var (fromUnit, toUnit) in knownRelations)
        {
            var fromNode = nodes[fromUnit];
            var toNode = nodes[toUnit];

            fromNode.AddNeighbor(toNode);
            toNode.AddNeighbor(fromNode);
        }

        return new ConversionMap(nodes.Values.ToImmutableHashSet());
    }
}
