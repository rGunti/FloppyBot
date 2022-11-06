using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route("api/v1/user")]
public class UserController : ControllerBase
{
    [HttpGet("me")]
    public UserDto Me()
    {
        throw this.NotImplemented();
    }

    [HttpGet("me/permissions")]
    public string[] GetMyPermissions()
    {
        throw this.NotImplemented();
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
