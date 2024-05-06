namespace FloppyBot.Chat.Twitch.Monitor;

public class TwitchChannelOnlineStatusChangedEventArgs : EventArgs
{
    public TwitchChannelOnlineStatusChangedEventArgs(TwitchStream? stream)
    {
        Stream = stream;
    }

    public TwitchStream? Stream { get; }
    public bool IsOnline => Stream?.IsOnline ?? false;
}
