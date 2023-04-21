using FloppyBot.Chat.Entities;

namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record CommandConfig(
    string ChannelId,
    string CommandName,
    PrivilegeLevel? PrivilegeLevelOverride,
    bool Disabled,
    bool Cooldown,
    CooldownConfig CooldownConfig
);
