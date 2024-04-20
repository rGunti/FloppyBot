namespace FloppyBot.Commands.Custom.Storage.Entities;

public record CommandResponse(ResponseType Type, string Content)
{
    public const char SOUND_CMD_SPLIT_CHAR = '\0';
}
