using System.Collections.Immutable;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Custom.Storage.Entities;

public record CommandLimitation
{
    private readonly IImmutableSet<CooldownDescription> _cooldownDescriptions =
        ImmutableSetWithValueSemantics<CooldownDescription>.Empty;

    private readonly IImmutableSet<string> _limitedUsers =
        ImmutableSetWithValueSemantics<string>.Empty;

    public PrivilegeLevel MinLevel { get; init; }

    public IImmutableSet<CooldownDescription> Cooldown
    {
        get => _cooldownDescriptions;
        init => _cooldownDescriptions = value.WithValueSemantics();
    }

    public IImmutableSet<string> LimitedToUsers
    {
        get => _limitedUsers;
        init => _limitedUsers = value.WithValueSemantics();
    }
}