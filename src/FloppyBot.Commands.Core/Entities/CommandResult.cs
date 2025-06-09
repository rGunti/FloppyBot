namespace FloppyBot.Commands.Core.Entities;

public record CommandResult(
    CommandOutcome Outcome,
    string? ResponseContent = null,
    bool SendAsReply = true
)
{
    public static readonly CommandResult Success = new(CommandOutcome.Success);
    public static readonly CommandResult Failed = new(CommandOutcome.Failed);
    public static readonly CommandResult Empty = new(CommandOutcome.NoResponse);
    public bool HasResponse => ResponseContent != null;

    public override string ToString()
    {
        return HasResponse
            ? $"{Outcome}: {ResponseContent ?? "empty"} ({nameof(SendAsReply)}={SendAsReply})"
            : $"{Outcome}";
    }

    public static implicit operator CommandResult(string? response)
    {
        return string.IsNullOrWhiteSpace(response) ? Empty : SuccessWith(response);
    }

    public static implicit operator string?(CommandResult result) => result.ResponseContent;

    public static CommandResult FailedWith(string content)
    {
        return Failed with { ResponseContent = content };
    }

    public static CommandResult FailedWith(string content, bool sendAsReply)
    {
        return Failed with { ResponseContent = content, SendAsReply = sendAsReply };
    }

    public static CommandResult SuccessWith(string content)
    {
        return Success with { ResponseContent = content };
    }

    public static CommandResult SuccessWith(string content, bool sendAsReply)
    {
        return Success with { ResponseContent = content, SendAsReply = sendAsReply };
    }
}
