using System.Reflection;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Internal;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Attributes.Args;

public class SourceMessageIdentifierAttribute : BaseArgumentAttribute
{
    public override object? ExtractArgument(ParameterInfo parameterInfo, CommandInstruction commandInstruction)
    {
        parameterInfo.AssertType<ChatMessageIdentifier>();
        return commandInstruction.Context!.SourceMessage.Identifier;
    }
}


