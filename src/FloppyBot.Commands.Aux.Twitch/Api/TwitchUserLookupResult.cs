namespace FloppyBot.Commands.Aux.Twitch.Api;

public record TwitchUserLookupResult(
    string AccountName,
    string DisplayName,
    string? LastGame)
{
    public string Link => $"https://twitch.tv/{AccountName}";
}
