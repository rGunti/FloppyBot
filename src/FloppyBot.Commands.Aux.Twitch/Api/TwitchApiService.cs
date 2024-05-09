using Microsoft.Extensions.Caching.Memory;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Teams;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Interfaces;

namespace FloppyBot.Commands.Aux.Twitch.Api;

public class TwitchApiService : ITwitchApiService
{
    private const string API_ERROR = "[API ERROR]";

    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(14);
    private static readonly TimeSpan CacheDurationShort = TimeSpan.FromMinutes(30);

    private readonly ITwitchAPI _twitchApi;
    private readonly IMemoryCache _memoryCache;

    public TwitchApiService(ITwitchAPI twitchApi, IMemoryCache memoryCache)
    {
        _twitchApi = twitchApi;
        _memoryCache = memoryCache;
    }

    public async Task<TwitchUserLookupResult?> LookupUser(string userId)
    {
        var userResponse = await GetUsersAsync(userId);
        if (userResponse is null || userResponse.Users.Length == 0)
        {
            return null;
        }

        var user = userResponse.Users.First();
        var channelResponse = await GetChannelInformationAsync(user.Id);

        return new TwitchUserLookupResult(
            user.Login,
            user.DisplayName,
            channelResponse?.Data.FirstOrDefault()?.GameName ?? API_ERROR
        );
    }

    public async Task<TwitchStreamTeamResult?> LookupTeam(string userId)
    {
        var userResponse = await GetUsersAsync(userId);
        if (userResponse is null || userResponse.Users.Length == 0)
        {
            return null;
        }

        var user = userResponse.Users.First();
        var channelTeams = await GetChannelTeamsAsync(user.Id);
        if (channelTeams is null || channelTeams.ChannelTeams.Length == 0)
        {
            return null;
        }

        var team = channelTeams.ChannelTeams.First();
        return new TwitchStreamTeamResult(
            user.DisplayName,
            team.Id,
            team.TeamName,
            team.TeamDisplayName
        );
    }

    private Task<GetUsersResponse?> GetUsersAsync(string loginName)
    {
        return _memoryCache.GetOrCreateAsync(
            $"twitch-user-{loginName}",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return await _twitchApi.Helix.Users.GetUsersAsync(logins: [loginName]);
            }
        );
    }

    private Task<GetChannelInformationResponse?> GetChannelInformationAsync(string userId)
    {
        return _memoryCache.GetOrCreateAsync(
            $"twitch-channel-{userId}",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDurationShort;
                return await _twitchApi.Helix.Channels.GetChannelInformationAsync(userId);
            }
        );
    }

    private Task<GetChannelTeamsResponse?> GetChannelTeamsAsync(string userId)
    {
        return _memoryCache.GetOrCreateAsync(
            $"twitch-teams-{userId}",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return await _twitchApi.Helix.Teams.GetChannelTeamsAsync(userId);
            }
        );
    }
}
