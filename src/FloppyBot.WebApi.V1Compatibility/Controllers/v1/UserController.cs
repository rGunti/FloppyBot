using AutoMapper;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

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
        throw this.NotImplemented();
    }

    [HttpGet("me/channels/{messageInterface}/{channel}")]
    public string GetChannelAlias(
        [FromRoute] string messageInterface,
        [FromRoute] string channel)
    {
        throw this.NotImplemented();
    }
}
