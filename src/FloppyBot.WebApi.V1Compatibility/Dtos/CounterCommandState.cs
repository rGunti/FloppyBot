namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record CounterCommandState(
    string Id,
    string ChannelId,
    string Name,
    string Response,
    int State);
