using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Aux.Twitch.Storage.Entities;

public record TimerMessageConfiguration(
    string Id,
    string[] Messages,
    int Interval,
    int MinMessages) : IEntity<TimerMessageConfiguration>
{
    public TimerMessageConfiguration() : this(null!, Array.Empty<string>(), -1, -1)
    {
    }

    public TimerMessageConfiguration WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}

