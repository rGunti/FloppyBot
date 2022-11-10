using FloppyBot.Base.Storage;
using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Custom.Storage.Entities.Internal;

public record CustomCommandDescriptionEo
    : IEntity<CustomCommandDescriptionEo>
{
    public string Name { get; set; }
    public string[] Aliases { get; set; }
    public CommandResponseEo[] Responses { get; set; }
    public CommandLimitationEo Limitations { get; set; }
    public string Id { get; set; }

    public CustomCommandDescriptionEo WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}

public record CommandResponseEo
{
    public string Type { get; set; }
    public string Content { get; set; }
}

public record CommandLimitationEo
{
    public PrivilegeLevel MinLevel { get; set; }
    public CooldownDescriptionEo[] Cooldown { get; set; }
}

public record CooldownDescriptionEo
{
    public PrivilegeLevel Level { get; set; }
    public int Milliseconds { get; set; }
}
