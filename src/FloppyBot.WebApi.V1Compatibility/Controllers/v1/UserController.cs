using AutoMapper;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.V1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public UserController(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet("me")]
    public UserDto Me()
    {
        return _mapper.Map<UserDto>(_userService.GetUserInfo(User.GetUserId(), true)!);
    }

    [HttpGet("me/permissions")]
    public string[] GetMyPermissions()
    {
        return User.GetUserPermissions().ToArray();
    }

    [HttpPut("me/channels")]
    public IActionResult UpdateAliases([FromBody] Dictionary<string, string> channelNames)
    {
        _userService.UpdateChannelAlias(User.GetUserId(), channelNames);
        return NoContent();
    }

    [HttpGet("me/channels/{messageInterface}/{channel}")]
    public ChannelAliasDto GetChannelAlias(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        var userInfo = _userService.GetUserInfo(User.GetUserId());
        var channelAlias = userInfo?.ChannelAliases.GetValueOrDefault(
            new ChannelIdentifier(messageInterface, channel)
        );

        if (channelAlias == null)
        {
            throw new NotFoundException("Channel ID not found");
        }

        return new ChannelAliasDto(channelAlias);
    }
}
