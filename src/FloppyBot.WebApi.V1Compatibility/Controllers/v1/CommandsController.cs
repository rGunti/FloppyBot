using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using FloppyBot.WebApi.V1Compatibility.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/commands")]
[Authorize(Policy = Permissions.READ_COMMANDS)]
public class CommandsController : ControllerBase
{
    private readonly V1CommandConverter _commandConverter;

    public CommandsController(V1CommandConverter commandConverter)
    {
        _commandConverter = commandConverter;
    }

    [HttpGet]
    public CommandInfo[] GetCommands()
    {
        return _commandConverter.GetAllKnownCommands()
            .OrderBy(c => c.Name)
            .ToArray();
    }
}
