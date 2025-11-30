using FloppyBot.Chat.Twitch.Events;

namespace FloppyBot.Chat.Twitch.EventSources;

public interface ITwitchEventSource
{
    event EventHandler<TwitchChannelPointsRewardRedeemedEvent> ChannelPointsCustomRewardRedemptionAdd;

    Task ConnectAsync();
    Task DisconnectAsync();
}
