namespace FloppyBot.Chat.Twitch.Monitor;

public delegate void TwitchChannelOnlineStatusChangedDelegate(
    ITwitchChannelOnlineMonitor sender,
    TwitchChannelOnlineStatusChangedEventArgs eventArgs
);

public delegate void TwitchChannelStatusChangedDelegate(
    ITwitchChannelOnlineMonitor sender,
    TwitchChannelOnlineStatusChangedEventArgs eventArgs
);
