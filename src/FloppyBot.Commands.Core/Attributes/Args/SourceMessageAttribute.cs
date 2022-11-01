using System.Reflection;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Internal;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Attributes.Args;

public class SourceMessageAttribute : BaseArgumentAttribute
{
    public override object? ExtractArgument(ParameterInfo parameterInfo, CommandInstruction commandInstruction)
    {
        parameterInfo.AssertType<ChatMessage>();
        return commandInstruction.Context!.SourceMessage;
    }
}
