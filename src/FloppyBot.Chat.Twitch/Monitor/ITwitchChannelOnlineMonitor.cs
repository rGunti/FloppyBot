namespace FloppyBot.Chat.Twitch.Monitor;

public interface ITwitchChannelOnlineMonitor
{
    TwitchStream? Stream { get; }
    bool IsChannelOnline();

    event TwitchChannelOnlineStatusChangedDelegate OnlineStatusChanged;
    event TwitchChannelStatusChangedDelegate StatusChanged;
}
