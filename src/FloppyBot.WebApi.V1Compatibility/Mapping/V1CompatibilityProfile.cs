using System.Collections.Immutable;
using AutoMapper;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;
using FloppyBot.WebApi.Auth.Dtos;
using FloppyBot.WebApi.V1Compatibility.Dtos;

namespace FloppyBot.WebApi.V1Compatibility.Mapping;

public class V1CompatibilityProfile : Profile
{
    public V1CompatibilityProfile()
    {
        MapQuote();
        MapUser();
        MapShoutoutMessage();
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
                q.CreatedBy))
            .ForAllMembers(o => o.Ignore());
        CreateMap<QuoteDto, Quote>()
            .ConstructUsing(dto => new Quote(
                dto.Id,
                dto.Channel,
                dto.QuoteId,
                dto.QuoteText,
                dto.QuoteContext ?? string.Empty,
                dto.CreatedAt,
                dto.CreatedBy))
            .ForAllMembers(o => o.Ignore());
    }

    private void MapUser()
    {
        CreateMap<User, UserDto>()
            .ConstructUsing(u => new UserDto(
                u.Id,
                u.OwnerOf.ToImmutableListWithValueSemantics(),
                u.ChannelAliases.ToImmutableDictionary()))
            .ForAllMembers(o => o.Ignore());
    }

    private void MapShoutoutMessage()
    {
        CreateMap<ShoutoutMessageConfig, ShoutoutMessageSetting>()
            .ConstructUsing(c => new ShoutoutMessageSetting(
                c.Id,
                c.Message));
        CreateMap<ShoutoutMessageSetting, ShoutoutMessageConfig>()
            .ConstructUsing(c => new ShoutoutMessageConfig(
                c.Id,
                c.Message));
    }
}
