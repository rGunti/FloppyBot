using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Core.Config;

public record CooldownConfiguration(
    PrivilegeLevel PrivilegeLevel,
    int CooldownMs);
