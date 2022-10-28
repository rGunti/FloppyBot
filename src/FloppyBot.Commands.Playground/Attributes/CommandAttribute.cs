using System.Collections.Immutable;
using FloppyBot.Base.EquatableCollections;

namespace FloppyBot.Commands.Playground.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class CommandAttribute : Attribute
{
    public CommandAttribute(params string[] commandNames)
    {
        CommandNames = commandNames.ToImmutableListWithValueSemantics();
    }

    public IImmutableList<string> CommandNames { get; init; }
}
