namespace FloppyBot.Chat.Twitch.Config;

public record TwitchConfiguration(
    string Username,
    string Token,
    string Channel,
    string ClientId,
    string AccessToken,
    bool DisableWhenChannelIsOffline,
    int MonitorInterval)
{
    public bool HasTwitchApiCredentials
        => !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(AccessToken);
}
