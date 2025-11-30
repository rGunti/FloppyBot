using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Chat.Twitch.Extensions;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace FloppyBot.Chat.Twitch.EventSources;

internal static class InternalEventConverters
{
    internal static TwitchChannelPointsRewardRedeemedEvent ConvertToInternalEvent(
        this ChannelPointsCustomRewardRedemption redemption
    )
    {
        return new TwitchChannelPointsRewardRedeemedEvent(
            redemption.Id,
            TwitchEntityExtensions.ConvertToChatUser(redemption.UserLogin, redemption.UserName),
            new TwitchChannelPointsReward(
                redemption.Reward.Id,
                redemption.Reward.Title,
                redemption.Reward.Prompt,
                redemption.Reward.Cost
            )
        );
    }
}
