namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record InvokeSoundCommandEvent(
    string InvokedBy,
    string InvokedFrom,
    string CommandName,
    DateTimeOffset InvokedAt);
