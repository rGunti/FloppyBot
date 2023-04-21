namespace FloppyBot.Commands.Aux.Twitch.Api;

public record TwitchUserLookupResult(string AccountName, string DisplayName, string? LastGame)
{
    public string Link => $"https://twitch.tv/{AccountName}";

    [Obsolete(
        "This property is only here for backwards compatibility and should not be used anymore"
    )]
    public string User => DisplayName;

    [Obsolete(
        "This property is only here for backwards compatibility and should not be used anymore"
    )]
    public string UserLink => Link;
}
