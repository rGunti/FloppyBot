using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.WebApi.V2.Dtos;

public record QuoteDto(
    string Id,
    string ChannelMappingId,
    int QuoteId,
    string QuoteText,
    string QuoteContext,
    DateTimeOffset CreatedAt,
    string CreatedBy
)
{
    public static QuoteDto FromQuote(Quote quote)
    {
        return new QuoteDto(
            quote.Id,
            quote.ChannelMappingId,
            quote.QuoteId,
            quote.QuoteText,
            quote.QuoteContext,
            quote.CreatedAt,
            quote.CreatedBy
        );
    }

    public Quote ToEntity()
    {
        return new Quote(
            Id,
            ChannelMappingId,
            QuoteId,
            QuoteText,
            QuoteContext,
            CreatedAt,
            CreatedBy
        );
    }
}
