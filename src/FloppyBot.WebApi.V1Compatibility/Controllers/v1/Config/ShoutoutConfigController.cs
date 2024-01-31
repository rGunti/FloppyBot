using AutoMapper;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Twitch.Storage;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.V1.Config;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/config/so")]
[Authorize(Policy = Permissions.READ_CONFIG)]
public class ShoutoutConfigController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IShoutoutMessageSettingService _shoutoutMessageSettingService;
    private readonly IUserService _userService;

    public ShoutoutConfigController(
        IShoutoutMessageSettingService shoutoutMessageSettingService,
        IUserService userService,
        IMapper mapper
    )
    {
        _shoutoutMessageSettingService = shoutoutMessageSettingService;
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet]
    public ShoutoutMessageConfig[] GetAllConfigs()
    {
        return _userService
            .GetAccessibleChannelsForUser(User.GetUserId())
            .Where(channelId => channelId.StartsWith("Twitch/"))
            .Select(channelId =>
                _shoutoutMessageSettingService.GetSettings(channelId)
                ?? new ShoutoutMessageSetting(channelId, string.Empty)
            )
            .Select(settings => _mapper.Map<ShoutoutMessageConfig>(settings))
            .ToArray();
    }

    [HttpPost]
    [Authorize(Policy = Permissions.EDIT_CONFIG)]
    public IActionResult UpdateAllConfigs([FromBody] ShoutoutMessageConfig[] configs)
    {
        var allowedChannelIds = _userService.GetAccessibleChannelsForUser(User.GetUserId());
        if (configs.Select(c => c.Id).Any(channelId => !allowedChannelIds.Contains(channelId)))
        {
            throw new ForbiddenException("You don't have access to all channels");
        }

        foreach (var (id, message) in configs)
        {
            _shoutoutMessageSettingService.SetShoutoutMessage(id, message);
        }

        return NoContent();
    }

    [HttpPost("{messageInterface}/{channel}")]
    [Authorize(Policy = Permissions.EDIT_CONFIG)]
    public IActionResult UpdateConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] ShoutoutMessageConfig config
    )
    {
        var channelId = new ChannelIdentifier(messageInterface, channel);
        if (
            _userService
                .GetAccessibleChannelsForUser(User.GetUserId())
                .All(c => c != channelId.ToString())
        )
        {
            throw new ForbiddenException("You don't have access to this channel");
        }

        _shoutoutMessageSettingService.SetShoutoutMessage(channelId, config.Message);
        return NoContent();
    }
}
