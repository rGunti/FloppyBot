namespace FloppyBot.Commands.Core.Entities;

public record CommandResult(
    CommandOutcome Outcome,
    string? ResponseContent = null)
{
    public bool HasResponse => ResponseContent != null;

    public override string ToString()
    {
        return HasResponse ? $"{Outcome}: {ResponseContent ?? "empty"}" : $"{Outcome}";
    }
}
