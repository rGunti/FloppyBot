#pragma warning disable CS8618
using System.Collections.Immutable;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Base.Storage;

namespace FloppyBot.Aux.TwitchAlerts.Core.Entities;

public record TwitchAlertSettings : IEntity<TwitchAlertSettings>
{
    private readonly IImmutableList<TwitchAlertMessage> _messages =
        ImmutableList<TwitchAlertMessage>.Empty;

    public string Id { get; init; }

    public bool SubAlertsEnabled { get; init; }

    public IImmutableList<TwitchAlertMessage> Messages
    {
        get => _messages;
        init => _messages = value.WithValueSemantics();
    }

    public TwitchAlertSettings WithId(string newId)
    {
        return this with { Id = newId };
    }
}

public record TwitchAlertMessage(
    string DefaultMessage,
    string? Tier1Message,
    string? Tier2Message,
    string? Tier3Message,
    string? PrimeMessage
);
