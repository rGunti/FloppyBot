#pragma warning disable CS8618
using System.Collections.Immutable;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Custom.Storage.Entities;

public record CustomCommandDescription : IEntity<CustomCommandDescription>
{
    private readonly IImmutableSet<string> _aliases = ImmutableSetWithValueSemantics<string>.Empty;
    private readonly IImmutableSet<string> _owner = ImmutableSetWithValueSemantics<string>.Empty;
    private readonly IImmutableList<CommandResponse> _responses =
        ImmutableList<CommandResponse>.Empty;

    public string Id { get; init; }

    public string Name { get; init; }

    public IImmutableSet<string> Aliases
    {
        get => _aliases;
        init => _aliases = value.ToImmutableSortedSetWithValueSemantics();
    }

    public IImmutableSet<string> Owners
    {
        get => _owner;
        init => _owner = value.ToImmutableSortedSetWithValueSemantics();
    }

    public IImmutableList<CommandResponse> Responses
    {
        get => _responses;
        init => _responses = value.WithValueSemantics();
    }

    public CommandLimitation Limitations { get; init; }

    public CommandResponseMode ResponseMode { get; init; }

    public bool IsSoundCommand =>
        Aliases.Count == 0 && Responses.All(r => r.Type == ResponseType.Sound);

    public CustomCommandDescription WithId(string newId)
    {
        return this with { Id = newId };
    }
}
