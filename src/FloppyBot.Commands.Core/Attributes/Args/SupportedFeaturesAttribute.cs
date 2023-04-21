using System.Reflection;
using FloppyBot.Chat;
using FloppyBot.Commands.Core.Internal;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Attributes.Args;

public class SupportedFeaturesAttribute : BaseArgumentAttribute
{
    public override object? ExtractArgument(
        ParameterInfo parameterInfo,
        CommandInstruction commandInstruction
    )
    {
        parameterInfo.AssertType<ChatInterfaceFeatures>();
        return commandInstruction.Context!.SourceMessage.SupportedFeatures;
    }
}
