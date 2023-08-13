namespace FloppyBot.Chat.Twitch.Events;

public record TwitchSubscriptionReceivedEvent(TwitchSubscriptionPlan SubscriptionPlanTier)
    : TwitchEvent(TwitchEventTypes.SUBSCRIPTION);
