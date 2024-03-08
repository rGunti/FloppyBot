using System.Collections.Immutable;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Implementation.PathFinding;

public class ConversionMap
{
    internal ConversionMap(ISet<ConversionNode> nodes)
    {
        Nodes = nodes.ToImmutableDictionary(i => i.Name, i => i);
    }

    public IImmutableDictionary<string, ConversionNode> Nodes { get; }

    public bool HasNode(string node) => Nodes.ContainsKey(node);

    public ConversionNode GetNode(string node) => Nodes[node];

    public bool IsNodeReachable(
        string originNode,
        string targetNode,
        out ConversionPath pathStack,
        int maxDepth = 128
    )
    {
        return IsNodeReachable(GetNode(originNode), GetNode(targetNode), out pathStack, maxDepth);
    }

    public bool IsNodeReachable(
        ConversionNode originNode,
        ConversionNode targetNode,
        out ConversionPath pathStack,
        int maxDepth = 128
    )
    {
        pathStack = new ConversionPath();
        return IsNodeReachable(originNode, originNode, targetNode, pathStack, maxDepth);
    }

    public bool IsNodeReachable(
        ConversionNode originNode,
        ConversionNode refNode,
        ConversionNode targetNode,
        ConversionPath pathStack,
        int maxDepth = 128
    )
    {
        if (originNode == targetNode)
        {
            throw new InvalidOperationException("Origin and Target node cannot be the same!");
        }

        pathStack.Push(refNode);
        foreach (var neighbor in refNode.Neighbors)
        {
            // If we found the origin, we're going circles
            if (neighbor == originNode)
            {
                continue;
            }

            // If we came from that neighbor, ignore it or we'll be going circles
            if (neighbor == refNode)
            {
                continue;
            }

            // If stack already contains this node, ignore it or we'll be going circles
            if (pathStack.Contains(neighbor))
            {
                continue;
            }

            // If we found the target, we're done!
            if (neighbor == targetNode)
            {
                return true;
            }

            // Check if we can reach the target from the current point
            if (maxDepth > 0)
            {
                var neighborCanReachTarget = IsNodeReachable(
                    originNode,
                    neighbor,
                    targetNode,
                    pathStack,
                    maxDepth - 1
                );
                if (neighborCanReachTarget)
                {
                    return true;
                }
            }
        }

        pathStack.Pop();
        return false;
    }
}
