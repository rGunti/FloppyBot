using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Config;

namespace FloppyBot.WebApi.V2.Dtos;

public record CooldownConfigurationDto(PrivilegeLevel PrivilegeLevel, int CooldownMs)
{
    public static CooldownConfigurationDto FromEntity(CooldownConfiguration entity)
    {
        return new CooldownConfigurationDto(entity.PrivilegeLevel, entity.CooldownMs);
    }
}
