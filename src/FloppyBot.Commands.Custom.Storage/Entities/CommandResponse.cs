namespace FloppyBot.Commands.Custom.Storage.Entities;

public record CommandResponse(ResponseType Type, string Content)
{
    public const char ReplySplitChar = '\0';
}
