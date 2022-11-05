using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Aux.Quotes.Storage.Entities;

public record QuoteChannelMappingJoinKeys(
    string Id,
    string MappingId,
    string ChannelId,
    DateTime ExpiresAt) : IEntity<QuoteChannelMappingJoinKeys>
{
    public QuoteChannelMappingJoinKeys WithId(string newId)
    {
        return this with { Id = newId };
    }
}
