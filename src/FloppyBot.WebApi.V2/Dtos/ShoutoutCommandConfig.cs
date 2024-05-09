using FloppyBot.Commands.Aux.Twitch.Storage.Entities;

namespace FloppyBot.WebApi.V2.Dtos;

public record ShoutoutCommandConfig(string Message, string? TeamMessage)
{
    public static readonly ShoutoutCommandConfig Empty = new(string.Empty, null);

    public static ShoutoutCommandConfig FromEntity(ShoutoutMessageSetting entity)
    {
        return new ShoutoutCommandConfig(entity.Message, entity.TeamMessage);
    }

    public ShoutoutMessageSetting ToEntity(string channelId)
    {
        return new ShoutoutMessageSetting(channelId, Message, TeamMessage);
    }
}
