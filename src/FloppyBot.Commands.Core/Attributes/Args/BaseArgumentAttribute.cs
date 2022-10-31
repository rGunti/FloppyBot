using System.Reflection;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Attributes.Args;

[AttributeUsage(AttributeTargets.Parameter)]
public abstract class BaseArgumentAttribute : Attribute
{
    public abstract object? ExtractArgument(ParameterInfo parameterInfo, CommandInstruction commandInstruction);
}
