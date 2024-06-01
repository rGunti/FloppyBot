using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Auditing.Abstraction.Entities;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.UserProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers.Admin;

[ApiController]
[Route("api/v2/admin/audit")]
[Authorize(Permissions.READ_AUDIT)]
public class AuditController : ControllerBase
{
    private readonly IAuditor _auditor;
    private readonly IUserService _userService;

    public AuditController(IUserService userService, IAuditor auditor)
    {
        _auditor = auditor;
        _userService = userService;
    }

    [HttpGet]
    public ActionResult<AuditRecord[]> GetRecords()
    {
        // TODO: Introduce filtering parameters (like seen with logs)
        var ownerOf = _userService.GetAccessibleChannelsForUser(User.GetUserId());
        return Ok(_auditor.GetAuditRecords(ownerOf.ToArray()));
    }
}
