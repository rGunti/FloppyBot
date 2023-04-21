#pragma warning disable CS8618
using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Custom.Storage.Entities.Internal;

public record CommandLimitationEo
{
    public PrivilegeLevel MinLevel { get; set; }
    public CooldownDescriptionEo[] Cooldown { get; set; }
    public string[]? LimitedToUsers { get; set; }
}
