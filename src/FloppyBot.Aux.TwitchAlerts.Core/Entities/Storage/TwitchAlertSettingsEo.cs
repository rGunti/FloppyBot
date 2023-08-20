using FloppyBot.Base.Storage;

namespace FloppyBot.Aux.TwitchAlerts.Core.Entities.Storage;

public record TwitchAlertSettingsEo(
    string Id,
    bool SubAlertsEnabled,
    TwitchAlertMessageEo[] SubMessages,
    TwitchAlertMessageEo[] ReSubMessages,
    TwitchAlertMessageEo[] GiftSubMessages,
    TwitchAlertMessageEo[] GiftSubCommunityMessages
) : IEntity<TwitchAlertSettingsEo>
{
    public TwitchAlertSettingsEo WithId(string newId)
    {
        return this with { Id = newId };
    }
}

public record TwitchAlertMessageEo(
    string DefaultMessage,
    string? Tier1Message,
    string? Tier2Message,
    string? Tier3Message,
    string? PrimeMessage
);
