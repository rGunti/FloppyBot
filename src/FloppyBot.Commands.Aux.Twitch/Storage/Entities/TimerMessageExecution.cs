using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Aux.Twitch.Storage.Entities;

public record TimerMessageExecution(
    string Id,
    DateTimeOffset LastExecutedAt,
    int MessageIndex) : IEntity<TimerMessageExecution>
{
    public TimerMessageExecution WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}
