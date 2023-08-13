using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Chat.Twitch.Events;
using TwitchLib.Client.Enums;

namespace FloppyBot.Chat.Twitch.Extensions;

internal static class TwitchEntityExtensions
{
    internal static ChatUser ConvertToChatUser(
        string username,
        string displayName,
        PrivilegeLevel privilegeLevel = PrivilegeLevel.Unknown
    )
    {
        return new ChatUser(
            new ChannelIdentifier(TwitchChatInterface.IF_NAME, username),
            displayName,
            privilegeLevel
        );
    }

    internal static TwitchSubscriptionPlan ConvertToPlan(
        this SubscriptionPlan plan,
        string? planName = null
    )
    {
        var tier = plan.ConvertToInternalEnum();
        return new TwitchSubscriptionPlan(tier, planName ?? tier.GetDefaultName());
    }

    private static TwitchSubscriptionPlanTier ConvertToInternalEnum(this SubscriptionPlan plan)
    {
        return plan switch
        {
            SubscriptionPlan.NotSet => TwitchSubscriptionPlanTier.Unknown,
            SubscriptionPlan.Prime => TwitchSubscriptionPlanTier.Prime,
            SubscriptionPlan.Tier1 => TwitchSubscriptionPlanTier.Tier1,
            SubscriptionPlan.Tier2 => TwitchSubscriptionPlanTier.Tier2,
            SubscriptionPlan.Tier3 => TwitchSubscriptionPlanTier.Tier3,
            _
                => throw new ArgumentOutOfRangeException(
                    nameof(plan),
                    plan,
                    "Subscription plan is not convertible"
                ),
        };
    }

    private static string GetDefaultName(this TwitchSubscriptionPlanTier planTier)
    {
        return planTier switch
        {
            TwitchSubscriptionPlanTier.Unknown => "Unknown",
            TwitchSubscriptionPlanTier.Prime => "Prime",
            TwitchSubscriptionPlanTier.Tier1 => "Tier 1",
            TwitchSubscriptionPlanTier.Tier2 => "Tier 2",
            TwitchSubscriptionPlanTier.Tier3 => "Tier 3",
            _ => planTier.ToString(),
        };
    }
}
