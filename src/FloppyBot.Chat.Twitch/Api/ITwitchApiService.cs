using FloppyBot.Chat.Twitch.Api.Dtos;
using TwitchLib.Api.Helix.Models.Teams;
using TwitchLib.Api.Interfaces;

namespace FloppyBot.Chat.Twitch.Api;

public interface ITwitchApiService
{
    IEnumerable<StreamTeam> GetStreamTeamsOfChannel(string channel);
    string? GetBroadcasterId(string channel);
}

public class TwitchApiService : ITwitchApiService
{
    private readonly ITwitchAPI _twitchApi;

    public TwitchApiService(ITwitchAPI twitchApi)
    {
        _twitchApi = twitchApi;
    }

    public IEnumerable<StreamTeam> GetStreamTeamsOfChannel(string channel)
    {
        return DoGetStreamTeamName(channel).GetAwaiter().GetResult();
    }

    public string? GetBroadcasterId(string channel)
    {
        return DoGetBroadcasterId(channel).GetAwaiter().GetResult();
    }

    private static StreamTeam ConvertToStreamTeam(Team team)
    {
        return new StreamTeam(team.Id, team.TeamName, team.TeamDisplayName);
    }

    private async Task<IEnumerable<StreamTeam>> DoGetStreamTeamName(string channel)
    {
        var broadcasterId = await DoGetBroadcasterId(channel);
        if (broadcasterId == null)
        {
            return Enumerable.Empty<StreamTeam>();
        }

        var teams = await _twitchApi.Helix.Teams.GetTeamsAsync(broadcasterId);
        return teams.Teams.Select(ConvertToStreamTeam);
    }

    private async Task<string?> DoGetBroadcasterId(string channel)
    {
        var program = await _twitchApi.Helix.Users.GetUsersAsync(logins: [channel]);
        return program.Users.Select(u => u.Id).FirstOrDefault();
    }
}
