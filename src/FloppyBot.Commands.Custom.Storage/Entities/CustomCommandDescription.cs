using System.Collections.Immutable;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Custom.Storage.Entities;

public record CustomCommandDescription : IEntity<CustomCommandDescription>
{
    private readonly IImmutableSet<string> _aliases = ImmutableSetWithValueSemantics<string>.Empty;
    private readonly IImmutableList<CommandResponse> _responses = ImmutableList<CommandResponse>.Empty;
    public string Name { get; init; }

    public IImmutableSet<string> Aliases
    {
        get => _aliases;
        init => _aliases = value.WithValueSemantics();
    }

    public IImmutableList<CommandResponse> Responses
    {
        get => _responses;
        init => _responses = value.WithValueSemantics();
    }

    public CommandLimitation Limitations { get; init; }

    public string Id { get; init; }

    public CustomCommandDescription WithId(string newId)
    {
        return this with { Id = newId };
    }
}
