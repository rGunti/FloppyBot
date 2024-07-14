namespace FloppyBot.Chat.Twitch.Events;

public static class TwitchEventTypes
{
    public const string SUBSCRIPTION = "Twitch.Subscription";
    public const string RE_SUBSCRIPTION = "Twitch.ReSubscription";
    public const string SUBSCRIPTION_GIFT = "Twitch.SubscriptionGift";
    public const string SUBSCRIPTION_GIFT_COMMUNITY = "Twitch.SubscriptionGiftCommunity";
    public const string RAID = "Twitch.Raid";

    public const string USER_JOINED = "Twitch.UserJoined";
    public const string USER_LEFT = "Twitch.UserLeft";
}
