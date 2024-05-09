namespace FloppyBot.Commands.Aux.Twitch.Api;

public interface ITwitchApiService
{
    Task<TwitchUserLookupResult?> LookupUser(string userId);
    Task<TwitchStreamTeamResult?> LookupTeam(string userId);
}
