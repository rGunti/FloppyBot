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
    private readonly IApiKeyService _apiKeyService;

    public UserController(IUserService userService, IApiKeyService apiKeyService)
    {
        _userService = userService;
        _apiKeyService = apiKeyService;
    }

    [HttpGet("me")]
    public UserReport Me()
    {
        var user =
            _userService.GetUserInfo(User.GetUserId(), true)
            ?? throw new UnauthorizedException("User not found");
        return UserReport.FromUser(
            user,
            User.GetUserPermissions().ToArray(),
            _apiKeyService.GetApiKeyForUser(User.GetUserId())?.CreatedAt
        );
    }

    [HttpGet("access-key")]
    public IActionResult GetAccessKey()
    {
        var apiKey = _apiKeyService.GetApiKeyForUser(User.GetUserId());
        return Ok(new AccessKeyReport(apiKey?.Token));
    }

    [HttpPost("access-key/generate")]
    public IActionResult GenerateAccessKey()
    {
        _apiKeyService.InvalidateApiKeysForUser(User.GetUserId());
        _apiKeyService.CreateApiKeyForUser(User.GetUserId());
        return NoContent();
    }
}
