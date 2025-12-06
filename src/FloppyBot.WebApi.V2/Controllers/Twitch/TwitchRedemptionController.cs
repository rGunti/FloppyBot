using FloppyBot.Commands.Aux.Twitch.Api;
using FloppyBot.Commands.Aux.Twitch.Entities;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.V2.Dtos.CustomCommands;
using FloppyBot.WebApi.V2.Dtos.Twitch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers.Twitch;

[ApiController]
[Route("api/v2/twitch/{channel}/rewards")]
[Authorize(Policy = Permissions.EDIT_CONFIG)]
public class TwitchRedemptionController : ChannelScopedController
{
    private const string IF_TWITCH = "Twitch";

    private readonly ITwitchApiService _twitchApiService;
    private readonly ICustomCommandService _customCommandService;

    public TwitchRedemptionController(
        IUserService userService,
        ITwitchApiService twitchApiService,
        ICustomCommandService customCommandService
    )
        : base(userService)
    {
        _twitchApiService = twitchApiService;
        _customCommandService = customCommandService;
    }

    [HttpGet]
    public async Task<TwitchRewardDto[]> GetChannelRedemptions([FromRoute] string channel)
    {
        var channelId = EnsureChannelAccess(IF_TWITCH, channel);
        var results = await _twitchApiService.GetChannelRewards(channelId);
        return results
            .Select(reward => new TwitchRewardDto(
                reward,
                _customCommandService
                    .GetCommandForReward(reward.Id, channelId)
                    .Select(command => CustomCommandDto.FromEntity(command, null))
                    .FirstOrDefault()
            ))
            .ToArray();
    }

    [HttpPut("{rewardId}/link/{commandName}")]
    public IActionResult LinkRedemptionToCommand(
        [FromRoute] string channel,
        [FromRoute] string rewardId,
        [FromRoute] string commandName
    )
    {
        var channelId = EnsureChannelAccess(IF_TWITCH, channel);
        var result = _customCommandService.LinkTwitchReward(rewardId, channelId, commandName);

        if (!result)
        {
            return BadRequest();
        }

        return NoContent();
    }

    [HttpDelete("{rewardId}/link")]
    public IActionResult UnlinkRedemptionFromCommand(
        [FromRoute] string channel,
        [FromRoute] string rewardId
    )
    {
        var channelId = EnsureChannelAccess(IF_TWITCH, channel);
        _customCommandService.UnlinkTwitchReward(rewardId, channelId);
        return NoContent();
    }
}
