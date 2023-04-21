using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Aux.Timer.Storage.Entities;

public record TimerRecord(
    string Id,
    string SourceMessage,
    string TimerMessage,
    DateTimeOffset TargetTime,
    DateTimeOffset CreatedAt,
    string CreatedBy
) : IEntity<TimerRecord>
{
    public TimerRecord WithId(string newId)
    {
        return this with { Id = newId };
    }
}
