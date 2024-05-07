namespace FloppyBot.Chat.Twitch.Events;

public record TwitchRaidEvent(
    string ChannelName,
    string ChannelDisplayName,
    int ViewerCount,
    StreamTeam? StreamTeam
) : TwitchEvent(TwitchEventTypes.RAID);

public record StreamTeam(string Name, string DisplayName);
