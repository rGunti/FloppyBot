#pragma warning disable CS8618
using System.Collections.Immutable;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Base.Storage;

namespace FloppyBot.Aux.TwitchAlerts.Core.Entities;

public record TwitchAlertSettings : IEntity<TwitchAlertSettings>
{
    private readonly IImmutableList<TwitchAlertMessage> _subMessages =
        ImmutableList<TwitchAlertMessage>.Empty;
    private readonly IImmutableList<TwitchAlertMessage> _reSubMessages =
        ImmutableList<TwitchAlertMessage>.Empty;
    private readonly IImmutableList<TwitchAlertMessage> _giftSubMessages =
        ImmutableList<TwitchAlertMessage>.Empty;
    private readonly IImmutableList<TwitchAlertMessage> _giftSubCommunityMessages =
        ImmutableList<TwitchAlertMessage>.Empty;
    private readonly IImmutableList<TwitchAlertMessage> _raidAlertMessage =
        ImmutableList<TwitchAlertMessage>.Empty;

    public string Id { get; init; }

    public bool SubAlertsEnabled { get; init; }

    public IImmutableList<TwitchAlertMessage> SubMessage
    {
        get => _subMessages;
        init => _subMessages = value.WithValueSemantics();
    }

    public IImmutableList<TwitchAlertMessage> ReSubMessage
    {
        get => _reSubMessages;
        init => _reSubMessages = value.WithValueSemantics();
    }

    public IImmutableList<TwitchAlertMessage> GiftSubMessage
    {
        get => _giftSubMessages;
        init => _giftSubMessages = value.WithValueSemantics();
    }

    public IImmutableList<TwitchAlertMessage> GiftSubCommunityMessage
    {
        get => _giftSubCommunityMessages;
        init => _giftSubCommunityMessages = value.WithValueSemantics();
    }

    public IImmutableList<TwitchAlertMessage> RaidAlertMessage
    {
        get => _raidAlertMessage;
        init => _raidAlertMessage = value.WithValueSemantics();
    }

    public TwitchAlertSettings WithId(string newId)
    {
        return this with { Id = newId };
    }
}

public record TwitchAlertMessage(
    string DefaultMessage,
    string? Tier1Message = null,
    string? Tier2Message = null,
    string? Tier3Message = null,
    string? PrimeMessage = null
);
