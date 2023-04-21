using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Attributes.Args;

/// <summary>
/// This attribute designates that the parameter denoted shall be assigned
/// to a list of parameters in the command instruction, starting from the provided
/// index and (optionally) ending at another index.
/// </summary>
public class ArgumentRangeAttribute : BaseArgumentAttribute
{
    public ArgumentRangeAttribute(
        int startIndex,
        int endIndex = int.MaxValue,
        string joinWith = " ",
        bool stopIfMissing = true,
        bool outputAsArray = false
    )
    {
        StartIndex = startIndex;
        StopIfMissing = stopIfMissing;
        JoinWith = joinWith;
        EndIndex = endIndex;
        OutputAsArray = outputAsArray;
    }

    public int StartIndex { get; }
    public int EndIndex { get; }
    public string JoinWith { get; }
    public bool StopIfMissing { get; }
    public bool OutputAsArray { get; }

    public override object? ExtractArgument(
        ParameterInfo parameterInfo,
        CommandInstruction commandInstruction
    )
    {
        var scopedArgs = commandInstruction.Parameters
            .Skip(StartIndex)
            .TakeWhile((_, i) => i < EndIndex)
            .ToArray();
        if (!scopedArgs.Any() && StopIfMissing)
        {
            throw new ArgumentOutOfRangeException(
                parameterInfo.Name!,
                $"Argument {parameterInfo.Name} was not supplied (looked at index between {StartIndex} - {EndIndex})"
            );
        }

        if (OutputAsArray)
        {
            return scopedArgs.ToImmutableArray();
        }

        return string.Join(JoinWith, scopedArgs);
    }
}
