using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Extensions;
using FloppyBot.FileStorage;
using FloppyBot.FileStorage.Auditing;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V2.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers;

[ApiController]
[Route("api/v2/files/{messageInterface}/{channel}")]
[Authorize(Policy = Permissions.READ_FILES)]
public class FilesController : ChannelScopedController
{
    private readonly IFileService _fileService;
    private readonly IAuditor _auditor;

    public FilesController(IUserService userService, IFileService fileService, IAuditor auditor)
        : base(userService)
    {
        _fileService = fileService;
        _auditor = auditor;
    }

    [HttpGet]
    public FileHeaderDto[] GetFiles([FromRoute] string messageInterface, [FromRoute] string channel)
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        return _fileService.GetFilesOf(channelId).Select(FileHeaderDto.FromFileHeader).ToArray();
    }

    [HttpPost]
    [Authorize(Permissions.EDIT_FILES)]
    public IActionResult UploadFile(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        IFormFile? file
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        if (file == null)
        {
            return BadRequest("No file provided");
        }

        if (file.Length > 5.MegaBytes())
        {
            return BadRequest("File is larger than allowed");
        }

        if (
            !_fileService.CreateFile(
                channelId,
                file.FileName,
                file.ContentType,
                file.OpenReadStream()
            )
        )
        {
            throw new ConflictException(
                "A file with the same name already exists or you don't have enough space to store this file"
            );
        }

        _auditor.FileCreated(User.AsChatUser(), channelId, file.FileName, file.Length);
        return NoContent();
    }

    [HttpDelete("{fileName}")]
    [Authorize(Permissions.EDIT_FILES)]
    public IActionResult DeleteFile(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string fileName
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        _fileService.DeleteFile(channelId, fileName);
        _auditor.FileDeleted(User.AsChatUser(), channelId, fileName);
        return NoContent();
    }

    [HttpGet("dl/{fileName}")]
    public IActionResult GetFileContent(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] string fileName
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        var fileHeader = _fileService
            .GetFile(channelId, fileName)
            .OrThrow(() => new NotFoundException($"File {channelId}/{fileName} does not exist"));

        using var ms = new MemoryStream();
        _fileService.GetStreamForFile(channelId, fileName).CopyTo(ms);
        return File(ms.GetBuffer(), fileHeader.MimeType);
    }

    [HttpGet("quota")]
    public FileStorageQuotaDto GetFileQuota(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        return FileStorageQuotaDto.FromQuota(
            channelId,
            _fileService.GetQuotaFor(channelId),
            _fileService.GetClaimedQuota(channelId)
        );
    }
}
