using System.Collections.Immutable;

namespace FloppyBot.Commands.Playground.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public abstract class CommandParameterAttribute : Attribute
{
    public abstract object? Parse(IImmutableList<string> sourceParameters);
}

public class CombinedParameterAttribute : CommandParameterAttribute
{
    public CombinedParameterAttribute(int startIndex = 0, int endIndex = int.MaxValue)
    {
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    public int StartIndex { get; }
    public int EndIndex { get; }

    public override object? Parse(IImmutableList<string> sourceParameters)
    {
        return string.Join(" ", sourceParameters
            .Skip(StartIndex)
            .TakeWhile((p, i) => i < EndIndex));
    }
}
