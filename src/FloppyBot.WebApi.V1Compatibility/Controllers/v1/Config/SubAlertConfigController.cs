using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1.Config;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/config/sub-alerts")]
public class SubAlertConfigController : ControllerBase
{
    [HttpGet]
    public SubAlertConfig[] GetAllConfigs()
    {
        throw this.NotImplemented();
    }

    [HttpPost]
    public IActionResult UpdateAllConfigs([FromBody] SubAlertConfig[] configs)
    {
        throw this.NotImplemented();
    }

    [HttpPost("{messageInterface}/{channel}")]
    public IActionResult UpdateConfig(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromBody] SubAlertConfig config
    )
    {
        throw this.NotImplemented();
    }
}
