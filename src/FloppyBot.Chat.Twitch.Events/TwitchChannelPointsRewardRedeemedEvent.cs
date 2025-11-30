using FloppyBot.Chat.Entities;

namespace FloppyBot.Chat.Twitch.Events;

public record TwitchChannelPointsRewardRedeemedEvent(
    string EventId,
    ChatUser User,
    TwitchChannelPointsReward Reward
) : TwitchEvent(TwitchEventTypes.CHANNEL_POINTS_REWARD_REDEEMED);

public record TwitchChannelPointsReward(
    string RewardId,
    string Title,
    string Prompt,
    int PointCost
);
