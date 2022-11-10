using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Custom.Storage.Entities.Internal;

public record CooldownDescriptionEo
{
    public PrivilegeLevel Level { get; set; }
    public int Milliseconds { get; set; }
}
