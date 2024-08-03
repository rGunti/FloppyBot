namespace FloppyBot.Chat.Twitch.Events;

public record TwitchUserJoinedEvent(string ChannelName, string UserName)
    : TwitchEvent(TwitchEventTypes.USER_JOINED);

public record TwitchUserLeftEvent(string ChannelName, string UserName)
    : TwitchEvent(TwitchEventTypes.USER_LEFT);
