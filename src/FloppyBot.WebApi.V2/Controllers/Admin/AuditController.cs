using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Auditing.Abstraction.Entities;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers.Admin;

[ApiController]
[Route("api/v2/admin/audit/{messageInterface}/{channel}")]
[Authorize(Permissions.READ_AUDIT)]
public class AuditController : ChannelScopedController
{
    private readonly IAuditor _auditor;

    public AuditController(IUserService userService, IAuditor auditor)
        : base(userService)
    {
        _auditor = auditor;
    }

    [HttpGet]
    public ActionResult<AuditRecord[]> GetRecords(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        return Ok(_auditor.GetAuditRecords(channelId));
    }
}
