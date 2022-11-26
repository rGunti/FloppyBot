using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Custom.Storage.Entities.Internal;

public record CounterEo(
        string Id,
        int Value)
    : IEntity<CounterEo>
{
    public CounterEo WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}
