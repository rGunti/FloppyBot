using FloppyBot.Base.Logging.MongoDb;
using FloppyBot.Base.Logging.MongoDb.Entities;
using FloppyBot.WebApi.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers.Admin;

[ApiController]
[Route("api/v2/admin/log")]
[Authorize(Permissions.READ_LOGS)]
public class LogController : ControllerBase
{
    private readonly LogService _logService;

    public LogController(LogService logService)
    {
        _logService = logService;
    }

    [HttpGet]
    public IActionResult GetLog([FromQuery] LogRecordSearchParameters searchParams)
    {
        return Ok(_logService.GetLog(searchParams));
    }

    [HttpPost]
    public IActionResult GetLogEntries([FromBody] LogRecordSearchParameters searchParams)
    {
        return Ok(_logService.GetLog(searchParams));
    }
}
