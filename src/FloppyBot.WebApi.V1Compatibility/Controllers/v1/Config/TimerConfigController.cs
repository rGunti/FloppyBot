using AutoMapper;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Twitch.Storage;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1.Config;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/config/timer")]
public class TimerConfigController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ITimerMessageConfigurationService _timerMessageConfigurationService;
    private readonly IUserService _userService;

    public TimerConfigController(
        IMapper mapper,
        IUserService userService,
        ITimerMessageConfigurationService timerMessageConfigurationService
    )
    {
        _mapper = mapper;
        _userService = userService;
        _timerMessageConfigurationService = timerMessageConfigurationService;
    }

    private ChannelIdentifier EnsureChannelAccess(ChannelIdentifier channelIdentifier)
    {
        if (
            !_userService
                .GetAccessibleChannelsForUser(User.GetUserId())
                .Contains(channelIdentifier.ToString())
        )
        {
            throw new NotFoundException(
                $"You don't have access to {channelIdentifier} or it doesn't exist"
            );
        }

        return channelIdentifier;
    }

    private ChannelIdentifier EnsureChannelAccess(string messageInterface, string channel)
    {
        var channelId = new ChannelIdentifier(messageInterface, channel);
        return EnsureChannelAccess(channelId);
    }

    [HttpGet]
    public TimerMessageConfig[] GetAll()
    {
        return _userService
            .GetAccessibleChannelsForUser(User.GetUserId())
            .Select(
                channelId =>
                    _timerMessageConfigurationService
                        .GetConfigForChannel(channelId)
                        .SingleOrDefault(
                            new TimerMessageConfiguration(channelId, Array.Empty<string>(), 5, 0)
                        )
            )
            .Select(_mapper.Map<TimerMessageConfig>)
            .ToArray();
    }

    [HttpPost]
    public IActionResult UpdateAll([FromBody] TimerMessageConfig[] configs)
    {
        // Ensuring access to all channels
        var _ = configs.Select(i => EnsureChannelAccess(i.Id)).ToArray();

        foreach (
            TimerMessageConfiguration config in configs.Select(
                _mapper.Map<TimerMessageConfiguration>
            )
        )
        {
            _timerMessageConfigurationService.UpdateConfigurationForChannel(config.Id, config);
        }

        return NoContent();
    }

    [HttpPost("{messageInterface}/{channel}")]
    public IActionResult UpdateConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] TimerMessageConfig config
    )
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        var convertedConfig = _mapper.Map<TimerMessageConfiguration>(config);
        _timerMessageConfigurationService.UpdateConfigurationForChannel(channelId, convertedConfig);
        return NoContent();
    }
}
