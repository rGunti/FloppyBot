using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Commands.Aux.Twitch;
using FloppyBot.Commands.Aux.Twitch.Storage;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.V2.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers;

[ApiController]
[Route("api/v2/commands/config/{messageInterface}/{channel}/timer")]
[Authorize(Policy = Permissions.READ_CONFIG)]
public class TimerMessageController : ChannelScopedController
{
    private readonly ITimerMessageConfigurationService _timerMessageConfigurationService;
    private readonly IAuditor _auditor;

    public TimerMessageController(
        IUserService userService,
        ITimerMessageConfigurationService timerMessageConfigurationService,
        IAuditor auditor
    )
        : base(userService)
    {
        _timerMessageConfigurationService = timerMessageConfigurationService;
        _auditor = auditor;
    }

    [HttpGet]
    public TimerMessageConfigurationDto Get(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);

        return _timerMessageConfigurationService
            .GetConfigForChannel(channelId)
            .Select(TimerMessageConfigurationDto.FromEntity)
            .FirstOrDefault(TimerMessageConfigurationDto.Empty(channelId));
    }

    [HttpPost]
    public IActionResult Update(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] TimerMessageConfigurationDto config
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        var entity = config.ToEntity();
        _timerMessageConfigurationService.UpdateConfigurationForChannel(channelId, entity);
        _auditor.TimerMessagesChanged(User.AsChatUser(), channelId, entity);
        return NoContent();
    }
}
