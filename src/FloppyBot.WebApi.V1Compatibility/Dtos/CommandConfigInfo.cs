namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record CommandConfigInfo(
    CommandInfo Info,
    object? Config);
