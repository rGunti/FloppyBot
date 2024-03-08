using FloppyBot.Base.Storage;
using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Core.Config;

public record CommandConfiguration : IEntity<CommandConfiguration>
{
    public string Id { get; init; } = null!;
    public string ChannelId { get; init; } = null!;
    public string CommandName { get; init; } = null!;
    public PrivilegeLevel? RequiredPrivilegeLevel { get; init; }
    public bool Disabled { get; init; }
    public bool CustomCooldown { get; init; }
    public CooldownConfiguration[] CustomCooldownConfiguration { get; init; } =
        Array.Empty<CooldownConfiguration>();

    public CommandConfiguration WithId(string newId)
    {
        return this with { Id = newId };
    }
}
