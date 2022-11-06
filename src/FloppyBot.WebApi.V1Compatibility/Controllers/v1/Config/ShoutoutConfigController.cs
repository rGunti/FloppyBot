using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1.Config;

[ApiController]
[Route("api/v1/config/so")]
public class ShoutoutConfigController : ControllerBase
{
    [HttpGet]
    public ShoutoutMessageConfig[] GetAllConfigs()
    {
        throw this.NotImplemented();
    }

    [HttpPost]
    public IActionResult UpdateAllConfigs([FromBody] ShoutoutMessageConfig[] configs)
    {
        throw this.NotImplemented();
    }

    [HttpPost("{messageInterface}/{channel}")]
    public IActionResult UpdateConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] ShoutoutMessageConfig config)
    {
        throw this.NotImplemented();
    }
}
