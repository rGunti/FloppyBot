using System.Reflection;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Internal;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Attributes.Args;

public class SourceChannelAttribute : BaseArgumentAttribute
{
    public override object? ExtractArgument(ParameterInfo parameterInfo, CommandInstruction commandInstruction)
    {
        var channelIdentifier = commandInstruction.Context!.SourceMessage.Identifier.GetChannel();
        if (parameterInfo.IsOfType<string>())
        {
            return channelIdentifier.ToString();
        }

        parameterInfo.AssertType<ChannelIdentifier>();
        return channelIdentifier;
    }
}
