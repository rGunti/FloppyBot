using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Config;

namespace FloppyBot.WebApi.V2.Dtos;

public record CommandConfigurationDto(
    string? Id,
    string ChannelId,
    string CommandName,
    PrivilegeLevel? RequiredPrivilegeLevel,
    bool Disabled,
    bool CustomCooldown,
    CooldownConfigurationDto[] CustomCooldownConfiguration
)
{
    public static CommandConfigurationDto FromEntity(CommandConfiguration entity)
    {
        return new CommandConfigurationDto(
            entity.Id,
            entity.ChannelId,
            entity.CommandName,
            entity.RequiredPrivilegeLevel,
            entity.Disabled,
            entity.CustomCooldown,
            entity.CustomCooldownConfiguration.Select(CooldownConfigurationDto.FromEntity).ToArray()
        );
    }

    public CommandConfiguration ToEntity()
    {
        return new CommandConfiguration
        {
            Id = Id!,
            ChannelId = ChannelId,
            CommandName = CommandName,
            RequiredPrivilegeLevel = RequiredPrivilegeLevel,
            Disabled = Disabled,
            CustomCooldown = CustomCooldown,
            CustomCooldownConfiguration = CustomCooldownConfiguration
                .Select(c => c.ToEntity())
                .ToArray(),
        };
    }
}
