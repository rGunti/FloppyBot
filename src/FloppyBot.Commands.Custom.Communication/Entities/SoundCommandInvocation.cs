namespace FloppyBot.Commands.Custom.Communication.Entities;

public record SoundCommandInvocation(
    string InvokedBy,
    string InvokedFrom,
    string CommandName,
    string PayloadToPlay,
    DateTimeOffset InvokedAt
);
