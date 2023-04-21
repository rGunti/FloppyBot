using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Interfaces;

namespace FloppyBot.Commands.Aux.Twitch.Api;

public class TwitchApiService : ITwitchApiService
{
    private readonly ITwitchAPI _twitchApi;

    public TwitchApiService(ITwitchAPI twitchApi)
    {
        _twitchApi = twitchApi;
    }

    public async Task<TwitchUserLookupResult?> LookupUser(string userId)
    {
        GetUsersResponse? userResponse = _twitchApi.Helix.Users
            .GetUsersAsync(logins: new List<string> { userId })
            .Result;

        if (userResponse.Users.Any())
        {
            var user = userResponse.Users.First();
            var channelResponse = await _twitchApi.Helix.Channels.GetChannelInformationAsync(
                user.Id
            );
            return new TwitchUserLookupResult(
                user.Login,
                user.DisplayName,
                channelResponse.Data.FirstOrDefault()?.GameName ?? "[API ERROR]"
            );
        }

        return null;
    }
}
