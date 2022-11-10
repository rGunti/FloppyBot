using System.Collections.Immutable;

namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record UserDto(
    string Id,
    IImmutableList<string> OwnerOf,
    IImmutableDictionary<string, string> ChannelAliases);
