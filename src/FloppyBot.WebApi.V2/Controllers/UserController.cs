using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V2.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers;

[ApiController]
[Route("api/v2/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public UserReport Me()
    {
        var user =
            _userService.GetUserInfo(User.GetUserId(), true)
            ?? throw new UnauthorizedException("User not found");
        return UserReport.FromUser(user, User.GetUserPermissions().ToArray());
    }
}
