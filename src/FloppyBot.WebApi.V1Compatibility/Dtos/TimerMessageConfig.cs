using System.Collections.Immutable;

namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record TimerMessageConfig(
    string Id,
    IImmutableList<string> Messages,
    int Interval,
    int MinMessages);
