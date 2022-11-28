using System.Collections.Immutable;

namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record SoundCommand(
    string? Id,
    string CommandName,
    string ChannelId,
    bool LimitedToMod,
    IImmutableList<string> LimitedToUsers,
    bool HideFromCommandList,
    int Cooldown,
    string? Response,
    IImmutableList<string> SoundFiles);
