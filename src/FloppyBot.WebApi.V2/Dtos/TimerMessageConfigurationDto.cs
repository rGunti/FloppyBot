using FloppyBot.Commands.Aux.Twitch.Storage.Entities;

namespace FloppyBot.WebApi.V2.Dtos;

public record TimerMessageConfigurationDto(
    string ChannelId,
    string[] Messages,
    int Interval,
    int MinMessages
)
{
    private static readonly TimerMessageConfigurationDto EmptyDto = new(string.Empty, [], 0, 0);

    public static TimerMessageConfigurationDto FromEntity(TimerMessageConfiguration entity)
    {
        return new TimerMessageConfigurationDto(
            entity.Id,
            entity.Messages,
            entity.Interval,
            entity.MinMessages
        );
    }

    public static TimerMessageConfigurationDto Empty(string channelId)
    {
        return EmptyDto with { ChannelId = channelId };
    }

    public TimerMessageConfiguration ToEntity()
    {
        return new TimerMessageConfiguration(ChannelId, Messages, Interval, MinMessages);
    }
}
