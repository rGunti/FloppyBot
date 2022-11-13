using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Core.Attributes.Metadata;

public class CommandCooldownAttribute : CommandMetadataAttribute
{
    public CommandCooldownAttribute(PrivilegeLevel maxLevel, int cooldownMs, bool perUser = true)
        : base(CommandMetadataTypes.COOLDOWN, $"{maxLevel},{cooldownMs},{perUser}")
    {
        MaxLevel = maxLevel;
        CooldownMs = cooldownMs;
        PerUser = perUser;
    }

    public PrivilegeLevel MaxLevel { get; }
    public int CooldownMs { get; }
    public bool PerUser { get; }
}
