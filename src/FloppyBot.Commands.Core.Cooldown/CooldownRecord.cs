using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Core.Cooldown;

public record CooldownRecord(
    string Id,
    DateTimeOffset ExecutedAt) : IEntity<CooldownRecord>
{
    public CooldownRecord WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}
