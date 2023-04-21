using System.Reflection;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Attributes.Args;

/// <summary>
/// This attribute designates that the parameter denoted shall be assigned
/// to the n-th parameter in the command instruction.
/// </summary>
public class ArgumentIndexAttribute : BaseArgumentAttribute
{
    public ArgumentIndexAttribute(int index, bool stopIfMissing = true)
    {
        Index = index;
        StopIfMissing = stopIfMissing;
    }

    public int Index { get; }
    public bool StopIfMissing { get; }

    public override object? ExtractArgument(
        ParameterInfo parameterInfo,
        CommandInstruction commandInstruction
    )
    {
        var scopedArgs = commandInstruction.Parameters.Skip(Index).ToArray();
        if (!scopedArgs.Any() && StopIfMissing)
        {
            throw new ArgumentOutOfRangeException(
                parameterInfo.Name!,
                $"Argument {parameterInfo.Name} was not supplied"
            );
        }

        return scopedArgs.FirstOrDefault();
    }
}
