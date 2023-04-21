using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.Attributes;

namespace FloppyBot.Commands.Aux.Quotes.Storage.Entities;

[IndexFields("ChannelQuote", nameof(ChannelMappingId), nameof(QuoteId))]
public record Quote(
    string Id,
    string ChannelMappingId,
    int QuoteId,
    string QuoteText,
    string QuoteContext,
    DateTimeOffset CreatedAt,
    string CreatedBy
) : IEntity<Quote>
{
    public Quote WithId(string newId)
    {
        return this with { Id = newId };
    }

    public override string ToString()
    {
        return $"Quote #{QuoteId}: {QuoteText} [{QuoteContext} @ {CreatedAt:yyyy-MM-dd}]";
    }
}
