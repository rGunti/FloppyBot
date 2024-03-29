namespace FloppyBot.Commands.Custom.Communication.Entities;

/// <summary>
/// An event that represents a sound command invocation.
/// </summary>
/// <param name="InvokedBy">The user who invoked the command</param>
/// <param name="InvokedFrom">The channel this command was invoked from</param>
/// <param name="CommandName">The name of the command that was invoked</param>
/// <param name="PayloadToPlay">The name of the payload to play</param>
/// <param name="InvokedAt">The date and time this command was invoked at</param>
public record SoundCommandInvocation(
    string InvokedBy,
    string InvokedFrom,
    string CommandName,
    string PayloadToPlay,
    DateTimeOffset InvokedAt
);
