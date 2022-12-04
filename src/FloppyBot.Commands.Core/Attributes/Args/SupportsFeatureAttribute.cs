using System.Reflection;
using FloppyBot.Chat;
using FloppyBot.Commands.Core.Internal;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Attributes.Args;

public class SupportsFeatureAttribute : BaseArgumentAttribute
{
    public SupportsFeatureAttribute(ChatInterfaceFeatures feature)
    {
        Feature = feature;
    }

    public ChatInterfaceFeatures Feature { get; }

    public override object? ExtractArgument(ParameterInfo parameterInfo, CommandInstruction commandInstruction)
    {
        parameterInfo.AssertType<bool>();
        return commandInstruction.Context!.SourceMessage.SupportedFeatures.HasFlag(Feature);
    }
}

