namespace FloppyBot.Commands.Custom.Communication.Entities;

/// <summary>
/// An event that represents a sound command invocation.
/// </summary>
/// <param name="Type">The type of payload to be sent</param>
/// <param name="InvokedBy">The user who invoked the command</param>
/// <param name="InvokedFrom">The channel this command was invoked from</param>
/// <param name="CommandName">The name of the command that was invoked</param>
/// <param name="PayloadToPlay">The name of the payload to play</param>
/// <param name="InvokedAt">The date and time this command was invoked at</param>
public record SoundCommandInvocation(
    PayloadType Type,
    string InvokedBy,
    string InvokedFrom,
    string CommandName,
    string PayloadToPlay,
    DateTimeOffset InvokedAt
);

public enum PayloadType
{
    Sound,
    Visual,
}
