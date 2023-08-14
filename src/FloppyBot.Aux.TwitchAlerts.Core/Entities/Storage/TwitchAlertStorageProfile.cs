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
                        dto.Messages
                            .Select(msg => ctx.Mapper.Map<TwitchAlertMessageEo>(msg))
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
                        Messages = eo.Messages
                            .Select(msg => ctx.Mapper.Map<TwitchAlertMessage>(msg))
                            .ToImmutableList(),
                    }
            );
        CreateMap<TwitchAlertMessageEo, TwitchAlertMessage>();
    }
}
