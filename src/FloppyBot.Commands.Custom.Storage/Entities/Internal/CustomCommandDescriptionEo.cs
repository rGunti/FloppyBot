#pragma warning disable CS8618
using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Custom.Storage.Entities.Internal;

public record CustomCommandDescriptionEo : IEntity<CustomCommandDescriptionEo>
{
    public string Name { get; set; }
    public string[] Aliases { get; set; }
    public string[] Owners { get; set; }
    public CommandResponseEo[] Responses { get; set; }
    public CommandLimitationEo Limitations { get; set; }
    public CommandResponseMode ResponseMode { get; init; }
    public string Id { get; set; }

    public CustomCommandDescriptionEo WithId(string newId)
    {
        return this with { Id = newId };
    }
}
