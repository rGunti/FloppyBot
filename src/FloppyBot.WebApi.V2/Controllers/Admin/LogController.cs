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

    [HttpPost]
    public ActionResult<LogRecord[]> GetLogEntries(
        [FromBody] LogRecordSearchParameters searchParams
    )
    {
        return Ok(_logService.GetLog(searchParams));
    }

    [HttpPost("stats")]
    public ActionResult<LogStats> GetStats([FromBody] LogRecordSearchParameters searchParams)
    {
        return _logService.GetLogStats(searchParams);
    }
}
