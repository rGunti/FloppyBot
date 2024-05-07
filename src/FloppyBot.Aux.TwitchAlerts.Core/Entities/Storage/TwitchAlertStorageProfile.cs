using System.Collections.Immutable;
using AutoMapper;

namespace FloppyBot.Aux.TwitchAlerts.Core.Entities.Storage;

public class TwitchAlertStorageProfile : Profile
{
    public TwitchAlertStorageProfile()
    {
        // dto -> eo
        CreateMap<TwitchAlertSettings, TwitchAlertSettingsEo>()
            .ConvertUsing(
                (dto, _, ctx) =>
                    new TwitchAlertSettingsEo(
                        dto.Id,
                        dto.SubAlertsEnabled,
                        dto.SubMessage.Select(msg => ctx.Mapper.Map<TwitchAlertMessageEo>(msg))
                            .ToArray(),
                        dto.ReSubMessage.Select(msg => ctx.Mapper.Map<TwitchAlertMessageEo>(msg))
                            .ToArray(),
                        dto.GiftSubMessage.Select(msg => ctx.Mapper.Map<TwitchAlertMessageEo>(msg))
                            .ToArray(),
                        dto.GiftSubCommunityMessage.Select(msg =>
                                ctx.Mapper.Map<TwitchAlertMessageEo>(msg)
                            )
                            .ToArray(),
                        dto.RaidAlertMessage.Select(msg =>
                                ctx.Mapper.Map<TwitchAlertMessageEo>(msg)
                            )
                            .ToArray()
                    )
            );
        CreateMap<TwitchAlertMessage, TwitchAlertMessageEo>();

        // eo -> dto
        CreateMap<TwitchAlertSettingsEo, TwitchAlertSettings>()
            .ConvertUsing(
                (eo, _, ctx) =>
                    new TwitchAlertSettings
                    {
                        Id = eo.Id,
                        SubAlertsEnabled = eo.SubAlertsEnabled,
                        SubMessage = eo
                            .SubMessages.Select(msg => ctx.Mapper.Map<TwitchAlertMessage>(msg))
                            .ToImmutableList(),
                        ReSubMessage = eo
                            .ReSubMessages.Select(msg => ctx.Mapper.Map<TwitchAlertMessage>(msg))
                            .ToImmutableList(),
                        GiftSubMessage = eo
                            .GiftSubMessages.Select(msg => ctx.Mapper.Map<TwitchAlertMessage>(msg))
                            .ToImmutableList(),
                        GiftSubCommunityMessage = eo
                            .GiftSubCommunityMessages.Select(msg =>
                                ctx.Mapper.Map<TwitchAlertMessage>(msg)
                            )
                            .ToImmutableList(),
                        RaidAlertMessage = eo
                            .RaidAlertMessages.Select(msg =>
                                ctx.Mapper.Map<TwitchAlertMessage>(msg)
                            )
                            .ToImmutableList(),
                    }
            );
        CreateMap<TwitchAlertMessageEo, TwitchAlertMessage>();
    }
}
