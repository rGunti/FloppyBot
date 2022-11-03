using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Aux.Quotes.Storage.Entities;

public record QuoteChannelMapping(
        string Id,
        string MappingId,
        string ChannelId,
        bool Confirmed)
    : IEntity<QuoteChannelMapping>
{
    public QuoteChannelMapping WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }

    public override string ToString()
    {
        return
            $"{nameof(QuoteChannelMapping)}({Id}, {ChannelId} => {MappingId}, {(Confirmed ? "confirmed" : "not confirmed")})";
    }
}
