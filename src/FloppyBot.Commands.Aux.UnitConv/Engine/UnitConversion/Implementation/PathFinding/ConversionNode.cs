namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Implementation.PathFinding;

public class ConversionNode
{
    internal ConversionNode(string name)
        : this(name, new HashSet<ConversionNode>()) { }

    internal ConversionNode(string name, ISet<ConversionNode> neighbors)
    {
        Name = name;
        Neighbors = neighbors;
    }

    public string Name { get; }
    public ISet<ConversionNode> Neighbors { get; }

    public bool IsNeighborOf(ConversionNode node) => Neighbors.Contains(node);

    public bool IsNeighborOf(string node) => Neighbors.Any(n => n.Name == node);

    public void AddNeighbor(ConversionNode node)
    {
        Neighbors.Add(node);
    }

    public override string ToString() => $"{Name} (n={Neighbors.Count})";
}
