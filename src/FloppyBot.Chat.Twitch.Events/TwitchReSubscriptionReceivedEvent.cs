namespace FloppyBot.Chat.Twitch.Events;

public record TwitchReSubscriptionReceivedEvent(
    TwitchSubscriptionPlan SubscriptionPlanTier,
    int Months
) : TwitchEvent(TwitchEventTypes.RE_SUBSCRIPTION);
