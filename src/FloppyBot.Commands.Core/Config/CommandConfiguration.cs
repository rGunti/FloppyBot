using FloppyBot.Base.Storage;
using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Core.Config;

public record CommandConfiguration(
        string Id,
        string ChannelId,
        string CommandName,
        PrivilegeLevel? RequiredPrivilegeLevel,
        bool Disabled,
        bool CustomCooldown,
        CooldownConfiguration[] CustomCooldownConfiguration)
    : IEntity<CommandConfiguration>
{
    public CommandConfiguration WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}
