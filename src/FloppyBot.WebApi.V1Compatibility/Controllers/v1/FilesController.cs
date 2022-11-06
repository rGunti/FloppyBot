using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/files/{messageInterface}/{channel}")]
public class FilesController : ControllerBase
{
    [HttpGet]
    public FileHeader[] GetFiles(
        [FromRoute] string messageInterface,
        [FromRoute] string channel)
    {
        throw this.NotImplemented();
    }

    [HttpPost]
    public IActionResult UploadFile(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        IFormFile? file)
    {
        throw this.NotImplemented();
    }

    [HttpGet("{fileName}")]
    public FileHeader GetFile(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string fileName)
    {
        throw this.NotImplemented();
    }

    [HttpDelete("{fileName}")]
    public IActionResult DeleteFile(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string fileName)
    {
        throw this.NotImplemented();
    }

    [HttpGet("dl/{fileName}")]
    public IActionResult GetFileContent(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string fileName)
    {
        throw this.NotImplemented();
    }

    [HttpGet("quota")]
    public FileStorageQuota GetFileQuota(
        [FromRoute] string messageInterface,
        [FromRoute] string channel)
    {
        throw this.NotImplemented();
    }
}
