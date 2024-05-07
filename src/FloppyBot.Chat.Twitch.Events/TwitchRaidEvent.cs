namespace FloppyBot.Chat.Twitch.Events;

public record TwitchRaidEvent(
    string ChannelName,
    string ChannelDisplayName,
    int ViewerCount,
    string? StreamTeamName
) : TwitchEvent(TwitchEventTypes.RAID);
