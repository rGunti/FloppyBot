using System.Reflection;
using FloppyBot.Commands.Core.Internal;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Attributes.Args;

public class SourceContextAttribute : BaseArgumentAttribute
{
    public override object? ExtractArgument(ParameterInfo parameterInfo, CommandInstruction commandInstruction)
    {
        parameterInfo.AssertType<string>();
        return commandInstruction.Context!.SourceMessage.Context;
    }
}
