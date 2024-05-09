using FloppyBot.Commands.Aux.Twitch.Storage;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.V2.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers;

[ApiController]
[Route("api/v2/commands/config/{messageInterface}/{channel}/shoutout")]
[Authorize(Policy = Permissions.READ_CONFIG)]
public class ShoutoutCommandConfigController : ChannelScopedController
{
    private readonly IShoutoutMessageSettingService _shoutoutMessageSettingService;

    public ShoutoutCommandConfigController(
        IUserService userService,
        IShoutoutMessageSettingService shoutoutMessageSettingService
    )
        : base(userService)
    {
        _shoutoutMessageSettingService = shoutoutMessageSettingService;
    }

    [HttpGet]
    public ShoutoutCommandConfig GetConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        var config = _shoutoutMessageSettingService.GetSettings(channelId);
        if (config is null)
        {
            return ShoutoutCommandConfig.Empty;
        }

        return ShoutoutCommandConfig.FromEntity(config);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.EDIT_CONFIG)]
    public IActionResult UpdateConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] ShoutoutCommandConfig config
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        _shoutoutMessageSettingService.SetShoutoutMessage(config.ToEntity(channelId));
        return NoContent();
    }
}
