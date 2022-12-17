using FloppyBot.WebApi.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/export")]
[Authorize(Permissions.READ_BOT)]
public class V1DataExportController : ControllerBase
{
    [HttpPost]
    public IActionResult Export()
    {
        throw new NotImplementedException("V1 Data Export is not supported on V2");
    }
}

