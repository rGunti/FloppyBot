using TwitchLib.Api.Helix.Models.ChannelPoints;

namespace FloppyBot.Commands.Aux.Twitch.Entities;

public record ChannelReward(string Id, string Title, int Cost, bool IsEnabled, bool IsPaused)
{
    internal static ChannelReward FromApiModel(CustomReward apiModel)
    {
        return new ChannelReward(
            apiModel.Id,
            apiModel.Title,
            apiModel.Cost,
            apiModel.IsEnabled,
            apiModel.IsPaused
        );
    }
}
