using AutoMapper;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.FileStorage;
using FloppyBot.FileStorage.Entities;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileHeader = FloppyBot.WebApi.V1Compatibility.Dtos.FileHeader;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/files/{messageInterface}/{channel}")]
[Authorize(Permissions.READ_FILES)]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public FilesController(
        IUserService userService,
        IFileService fileService,
        IMapper mapper)
    {
        _userService = userService;
        _fileService = fileService;
        _mapper = mapper;
    }

    private void EnsureChannelAccess(ChannelIdentifier channelIdentifier)
    {
        if (!_userService.GetAccessibleChannelsForUser(User.GetUserId())
                .Contains(channelIdentifier.ToString()))
        {
            throw new NotFoundException($"You don't have access to {channelIdentifier} or it doesn't exist");
        }
    }

    private ChannelIdentifier EnsureChannelAccess(string messageInterface, string channel)
    {
        var channelId = new ChannelIdentifier(messageInterface, channel);
        EnsureChannelAccess(channelId);
        return channelId;
    }

    [HttpGet]
    public FileHeader[] GetFiles(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel)
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        return _fileService.GetFilesOf(channelId)
            .Select(f => _mapper.Map<FileHeader>(f))
            .OrderBy(f => f.FileName.ToLowerInvariant())
            .ToArray();
    }

    [HttpPost]
    [Authorize(Permissions.EDIT_FILES)]
    public IActionResult UploadFile(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel,
        IFormFile? file)
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        if (file == null)
        {
            throw new BadRequestException("No file provided");
        }

        if (file.Length > 5.MegaBytes())
        {
            throw new PayloadTooLargeException("File is larger than allowed");
        }

        if (!_fileService.CreateFile(channelId, file.FileName, file.ContentType, file.OpenReadStream()))
        {
            throw new ConflictException(
                "A file with the same name already exists or you don't have enough space to store this file");
        }

        return NoContent();
    }

    [HttpGet("{fileName}")]
    public FileHeader GetFile(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel,
        [FromRoute]
        string fileName)
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        return _fileService.GetFile(channelId, fileName)
            .Select(f => _mapper.Map<FileHeader>(f))
            .OrThrow(() => new NotFoundException($"File {channelId}/{fileName} does not exist"))
            .First();
    }

    [HttpDelete("{fileName}")]
    public IActionResult DeleteFile(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel,
        [FromRoute]
        string fileName)
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        _fileService.DeleteFile(channelId, fileName);
        return NoContent();
    }

    [HttpGet("dl/{fileName}")]
    public IActionResult GetFileContent(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel,
        [FromRoute]
        string fileName)
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        FileStorage.Entities.FileHeader fileHeader = _fileService.GetFile(channelId, fileName)
            .OrThrow(() => new NotFoundException($"File {channelId}/{fileName} does not exist"));

        using var ms = new MemoryStream();
        _fileService.GetStreamForFile(channelId, fileName).CopyTo(ms);
        return File(ms.GetBuffer(), fileHeader.MimeType);
    }

    [HttpGet("quota")]
    public FileStorageQuota GetFileQuota(
        [FromRoute] string messageInterface,
        [FromRoute]
        string channel)
    {
        ChannelIdentifier channelId = EnsureChannelAccess(messageInterface, channel);
        FileQuota quota = _fileService.GetQuotaFor(channelId);
        FileQuota usedQuota = _fileService.GetClaimedQuota(channelId);
        return new FileStorageQuota(
            channelId,
            quota.MaxStorageQuota,
            quota.MaxFileNumber,
            usedQuota.MaxStorageQuota,
            usedQuota.MaxFileNumber);
    }
}
