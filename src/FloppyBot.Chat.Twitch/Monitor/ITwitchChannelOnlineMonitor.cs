namespace FloppyBot.Chat.Twitch.Monitor;

public interface ITwitchChannelOnlineMonitor
{
    TwitchStream? Stream { get; }
    bool IsChannelOnline();
}
