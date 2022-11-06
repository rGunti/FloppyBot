namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record CounterCommandConfig(
    string Id,
    string ChannelId,
    string Name,
    string Response);
