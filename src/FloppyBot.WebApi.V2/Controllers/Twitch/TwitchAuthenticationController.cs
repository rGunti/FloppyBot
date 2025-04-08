using FloppyBot.TwitchApi;
using FloppyBot.TwitchApi.Exceptions;
using FloppyBot.TwitchApi.Storage;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.V2.Dtos.Twitch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers.Twitch;

[ApiController]
[Route("api/v2/twitch/{channel}/auth")]
[Authorize(Policy = Permissions.EDIT_CONFIG)]
public class TwitchAuthenticationController : ChannelScopedController
{
    private const string IF_TWITCH = "twitch";

    private readonly TwitchAuthenticator _twitchAuthenticator;

    public TwitchAuthenticationController(
        IUserService userService,
        ITwitchAccessCredentialInitiationService initiationService,
        TwitchAuthenticator twitchAuthenticator
    )
        : base(userService)
    {
        _twitchAuthenticator = twitchAuthenticator;
    }

    [HttpGet]
    public ActionResult<TwitchAuthenticationStart> StartAuthentication([FromRoute] string channel)
    {
        var channelId = EnsureChannelAccess(IF_TWITCH, channel);
        var returnUrl = _twitchAuthenticator.InitiateNewSession(User.GetUserId(), channelId);
        return new TwitchAuthenticationStart(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmAuthentication(
        [FromRoute] string channel,
        [FromBody] TwitchAuthenticationConfirm confirmation
    )
    {
        var channelId = EnsureChannelAccess(IF_TWITCH, channel);

        try
        {
            await _twitchAuthenticator.ConfirmSession(
                User.GetUserId(),
                channelId,
                confirmation.SessionId,
                confirmation.Code
            );
            return Accepted();
        }
        catch (TwitchAuthenticationException)
        {
            return BadRequest();
        }
    }
}
