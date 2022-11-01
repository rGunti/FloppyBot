using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Aux.Quotes.Storage.Entities;

public record Quote(
    string Id,
    string ChannelMappingId,
    int QuoteId,
    string QuoteText,
    string QuoteContext,
    DateTimeOffset CreatedAt,
    string CreatedBy) : IEntity
{
    public override string ToString()
    {
        return $"Quote #{QuoteId}: {QuoteText} [{QuoteContext} @ {CreatedAt:yyyy-MM-dd}]";
    }
}
