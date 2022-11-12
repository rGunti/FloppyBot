namespace FloppyBot.Commands.Core.Entities;

public record CommandResult(
    CommandOutcome Outcome,
    string? ResponseContent = null)
{
    public bool HasResponse => ResponseContent != null;
}

public enum CommandOutcome
{
    NoResponse,
    Success,
    Failed
}
