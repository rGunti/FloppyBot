using FloppyBot.TwitchApi;
using FloppyBot.TwitchApi.Exceptions;
using FloppyBot.TwitchApi.Storage;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V2.Dtos.Twitch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers.Twitch;

[ApiController]
[Route("api/v2/twitch")]
[Authorize(Policy = Permissions.EDIT_CONFIG)]
public class TwitchAuthenticationController : ChannelScopedController
{
    private const string IF_TWITCH = "Twitch";

    private readonly TwitchAuthenticator _twitchAuthenticator;

    public TwitchAuthenticationController(
        IUserService userService,
        TwitchAuthenticator twitchAuthenticator
    )
        : base(userService)
    {
        _twitchAuthenticator = twitchAuthenticator;
    }

    [HttpGet("{channel}")]
    public ActionResult<bool> HasAuthentication(string channel)
    {
        var channelId = EnsureChannelAccess(IF_TWITCH, channel);
        return Ok(_twitchAuthenticator.HasLinkForChannel(channelId));
    }

    [HttpGet("{channel}/auth")]
    public ActionResult<TwitchAuthenticationStart> StartAuthentication([FromRoute] string channel)
    {
        var channelId = EnsureChannelAccess(IF_TWITCH, channel);
        var returnUrl = _twitchAuthenticator.InitiateNewSession(User.GetUserId(), channelId);
        return new TwitchAuthenticationStart(returnUrl);
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmAuthentication(
        [FromBody] TwitchAuthenticationConfirm confirmation
    )
    {
        var channelId = _twitchAuthenticator.GetChannelForSession(confirmation.SessionId);
        if (channelId is null)
        {
            return BadRequest("Session not found");
        }

        EnsureChannelAccess(channelId);

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

    [HttpDelete("{channel}/auth")]
    public ActionResult DeleteLink([FromRoute] string channel)
    {
        var channelId = EnsureChannelAccess(IF_TWITCH, channel);
        _twitchAuthenticator.RevokeCredentials(channelId);
        return Accepted();
    }
}
