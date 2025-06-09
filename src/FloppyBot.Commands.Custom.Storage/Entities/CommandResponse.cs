namespace FloppyBot.Commands.Custom.Storage.Entities;

public record CommandResponse(ResponseType Type, string Content, bool SendAsReply = true)
{
    public const char ReplySplitChar = '\0';
}
