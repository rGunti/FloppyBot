using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Core.Config;

public record CooldownConfiguration
{
    public PrivilegeLevel PrivilegeLevel { get; init; }
    public int CooldownMs { get; init; }
}
