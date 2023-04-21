using System.Collections.Immutable;

namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record CustomCommand(
    string? Id,
    string Channel,
    string Command,
    string Response,
    IImmutableList<string>? ResponseVariants,
    bool LimitedToMod,
    IImmutableList<string> LimitedToUsers,
    int Timeout
);
