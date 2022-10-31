using System.Reflection;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Attributes.Args;

/// <summary>
/// This attribute denotes a parameter which will contain
/// all parameters requested by the command instruction
/// </summary>
public class AllArgumentsAttribute : BaseArgumentAttribute
{
    public AllArgumentsAttribute(string joinWith = " ")
    {
        JoinWith = joinWith;
    }

    public string JoinWith { get; }

    public override object? ExtractArgument(ParameterInfo parameterInfo, CommandInstruction commandInstruction)
    {
        return parameterInfo.ParameterType == typeof(string)
            ? string.Join(JoinWith, commandInstruction.Parameters)
            : commandInstruction.Parameters;
    }
}
