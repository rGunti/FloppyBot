using FloppyBot.Commands.Aux.Twitch.Storage.Entities;

namespace FloppyBot.WebApi.V2.Dtos;

public record ShoutoutCommandConfig(string Message)
{
    public static readonly ShoutoutCommandConfig Empty = new(string.Empty);

    public static ShoutoutCommandConfig FromEntity(ShoutoutMessageSetting entity)
    {
        return new ShoutoutCommandConfig(entity.Message);
    }
}
