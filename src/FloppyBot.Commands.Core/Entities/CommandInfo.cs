using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Commands.Core.Attributes.Metadata;

namespace FloppyBot.Commands.Core.Entities;

public record CommandInfo
{
    public CommandInfo(IEnumerable<string> names, MethodInfo handlerMethod)
    {
        Names = names.ToImmutableSortedSetWithValueSemantics();
        HandlerMethod = handlerMethod;
    }

    /// <summary>
    /// Returns a set of names associated with this command
    /// </summary>
    public IImmutableSet<string> Names { get; }

    /// <summary>
    /// Returns the method that is used as the handler of the command
    /// </summary>
    public MethodInfo HandlerMethod { get; }

    /// <summary>
    /// If set, this method can be called directly without
    /// constructing an instance
    /// </summary>
    public bool IsStatic => HandlerMethod.IsStatic;

    /// <summary>
    /// Returns the type of the class that implements this command handler
    /// </summary>
    public Type ImplementingType => HandlerMethod.ReflectedType!;

    /// <summary>
    /// Returns an ID that identifies the command
    /// </summary>
    public string CommandId => PrimaryCommandName;

    /// <summary>
    /// Returns the primary name of this command
    /// </summary>
    public string PrimaryCommandName
        => HandlerMethod.GetCustomAttribute<PrimaryCommandNameAttribute>()?.Value
           ?? Names.First();

    /// <summary>
    /// Returns true, if the command is variable
    /// </summary>
    public virtual bool IsVariable => false;

    public override string ToString()
    {
        return
            $"Cmd {PrimaryCommandName} [Alias={string.Join(",", Names)}]{(IsStatic ? " (static)" : "")} => {HandlerMethod}";
    }
}
