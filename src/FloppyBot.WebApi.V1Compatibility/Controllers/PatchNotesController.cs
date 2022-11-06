using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/patch-notes")]
public class PatchNotesController : ControllerBase
{
    [HttpGet]
    public PatchNote[] Get()
    {
        throw this.NotImplemented();
    }
}
