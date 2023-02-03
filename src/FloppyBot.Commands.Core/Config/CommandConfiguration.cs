using FloppyBot.Base.Storage;
using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Core.Config;

public record CommandConfiguration
    : IEntity<CommandConfiguration>
{
    public string Id { get; init; }
    public string ChannelId { get; init; }
    public string CommandName { get; init; }
    public PrivilegeLevel? RequiredPrivilegeLevel { get; init; }
    public bool Disabled { get; init; }
    public bool CustomCooldown { get; init; }
    public CooldownConfiguration[] CustomCooldownConfiguration { get; init; }

    public CommandConfiguration WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}
