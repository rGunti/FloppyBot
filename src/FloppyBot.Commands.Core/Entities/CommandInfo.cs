using System.Collections.Immutable;
using System.Reflection;

namespace FloppyBot.Commands.Core.Entities;

public record CommandInfo(
    IImmutableList<string> Names,
    MethodInfo HandlerMethod)
{
    /// <summary>
    /// If set, this method can be called directly without
    /// constructing an instance
    /// </summary>
    public bool IsStatic => HandlerMethod.IsStatic;

    /// <summary>
    /// Returns the type of the class that implements this command handler
    /// </summary>
    public Type ImplementingType => HandlerMethod.ReflectedType!;

    public override string ToString()
    {
        return $"Cmd: [{string.Join(",", Names)}]{(IsStatic ? " (static)" : "")} => {HandlerMethod}";
    }
}
