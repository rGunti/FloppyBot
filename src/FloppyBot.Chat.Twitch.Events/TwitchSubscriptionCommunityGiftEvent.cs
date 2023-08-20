namespace FloppyBot.Chat.Twitch.Events;

public record TwitchSubscriptionCommunityGiftEvent(
    TwitchSubscriptionPlan SubscriptionPlanTier,
    int MassGiftCount,
    string MultiMonthDuration
) : TwitchEvent(TwitchEventTypes.SUBSCRIPTION_GIFT_COMMUNITY);
