using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.Attributes;

namespace FloppyBot.Aux.MessageCounter.Core.Entities;

[IndexFields("ChannelLookup", nameof(ChannelId), nameof(OccurredAt))]
public record MessageOccurrence(
        string Id,
        string ChannelId,
        string AuthorId,
        DateTimeOffset OccurredAt)
    : IEntity<MessageOccurrence>
{
    public MessageOccurrence WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}


