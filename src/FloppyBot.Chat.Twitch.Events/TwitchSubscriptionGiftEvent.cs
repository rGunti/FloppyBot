using FloppyBot.Chat.Entities;

namespace FloppyBot.Chat.Twitch.Events;

public record TwitchSubscriptionGiftEvent(
    TwitchSubscriptionPlan SubscriptionPlanTier,
    ChatUser Recipient
) : TwitchEvent(TwitchEventTypes.SUBSCRIPTION_GIFT);
