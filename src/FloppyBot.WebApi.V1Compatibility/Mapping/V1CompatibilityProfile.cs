using AutoMapper;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;
using FloppyBot.WebApi.V1Compatibility.Dtos;

namespace FloppyBot.WebApi.V1Compatibility.Mapping;

public class V1CompatibilityProfile : Profile
{
    public V1CompatibilityProfile()
    {
        MapQuote();
    }

    private void MapQuote()
    {
        CreateMap<Quote, QuoteDto>()
            .ConstructUsing(q => new QuoteDto(
                q.Id,
                q.ChannelMappingId,
                q.QuoteId,
                q.QuoteText,
                q.QuoteContext,
                q.CreatedAt,
                q.CreatedBy));
        CreateMap<QuoteDto, Quote>()
            .ConstructUsing(dto => new Quote(
                dto.Id,
                dto.Channel,
                dto.QuoteId,
                dto.QuoteText,
                dto.QuoteContext ?? string.Empty,
                dto.CreatedAt,
                dto.CreatedBy));
    }
}
