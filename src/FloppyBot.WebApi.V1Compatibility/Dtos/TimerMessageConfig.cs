namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record TimerMessageConfig(string Id, string[] Messages, int Interval, int MinMessages);
