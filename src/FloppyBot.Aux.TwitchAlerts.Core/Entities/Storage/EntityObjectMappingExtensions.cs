using System.Collections.Immutable;
using FloppyBot.Aux.TwitchAlerts.Core.Entities;

namespace FloppyBot.Aux.TwitchAlerts.Core.Entities.Storage;

internal static class EntityObjectMappingExtensions
{
    public static TwitchAlertMessageEo ToEo(this TwitchAlertMessage dto) =>
        new(
            dto.DefaultMessage,
            dto.Tier1Message,
            dto.Tier2Message,
            dto.Tier3Message,
            dto.PrimeMessage
        );

    public static TwitchAlertMessage ToDto(this TwitchAlertMessageEo eo) =>
        new(eo.DefaultMessage, eo.Tier1Message, eo.Tier2Message, eo.Tier3Message, eo.PrimeMessage);

    public static TwitchAlertSettingsEo ToEo(this TwitchAlertSettings dto) =>
        new(
            dto.Id,
            dto.SubAlertsEnabled,
            dto.SubMessage.Select(m => m.ToEo()).ToArray(),
            dto.ReSubMessage.Select(m => m.ToEo()).ToArray(),
            dto.GiftSubMessage.Select(m => m.ToEo()).ToArray(),
            dto.GiftSubCommunityMessage.Select(m => m.ToEo()).ToArray(),
            dto.RaidAlertMessage.Select(m => m.ToEo()).ToArray()
        );

    public static TwitchAlertSettings? ToDto(this TwitchAlertSettingsEo? eo) =>
        eo is null
            ? null
            : new()
            {
                Id = eo.Id,
                SubAlertsEnabled = eo.SubAlertsEnabled,
                SubMessage = eo.SubMessages.Select(m => m.ToDto()).ToImmutableList(),
                ReSubMessage = eo.ReSubMessages.Select(m => m.ToDto()).ToImmutableList(),
                GiftSubMessage = eo.GiftSubMessages.Select(m => m.ToDto()).ToImmutableList(),
                GiftSubCommunityMessage = eo
                    .GiftSubCommunityMessages.Select(m => m.ToDto())
                    .ToImmutableList(),
                RaidAlertMessage = eo.RaidAlertMessages.Select(m => m.ToDto()).ToImmutableList(),
            };
}
