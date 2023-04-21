using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Custom.Storage.Entities;

public record CooldownDescription(PrivilegeLevel Level, int Milliseconds);
