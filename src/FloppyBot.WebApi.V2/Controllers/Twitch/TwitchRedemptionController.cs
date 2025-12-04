using FloppyBot.Commands.Aux.Twitch.Api;
using FloppyBot.Commands.Aux.Twitch.Entities;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers.Twitch;

[ApiController]
[Route("api/v2/twitch")]
[Authorize(Policy = Permissions.EDIT_CONFIG)]
public class TwitchRedemptionController : ChannelScopedController
{
    private const string IF_TWITCH = "Twitch";

    private readonly ITwitchApiService _twitchApiService;

    public TwitchRedemptionController(IUserService userService, ITwitchApiService twitchApiService)
        : base(userService)
    {
        _twitchApiService = twitchApiService;
    }

    [HttpGet("{channel}/redemptions")]
    public async Task<ActionResult<ChannelReward[]>> GetChannelRedemptions(
        [FromRoute] string channel
    )
    {
        var channelId = EnsureChannelAccess(IF_TWITCH, channel);
        var results = await _twitchApiService.GetChannelRewards(channelId);
        return Ok(results);
    }
}
