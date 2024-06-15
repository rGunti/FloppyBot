using FloppyBot.Base.Extensions;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.FileStorage;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.Dtos;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers;

[ApiController]
[Route("api/v1/stream-source/{messageInterface}/{channel}")] // todo: remove this link
[Route("api/v2/stream-source/{messageInterface}/{channel}")]
[Authorize(Policy = "ApiKey")]
public class StreamSourceController : ChannelScopedController
{
    private readonly IFileService _fileService;
    private readonly ICustomCommandService _customCommandService;

    public StreamSourceController(
        IFileService fileService,
        ICustomCommandService customCommandService,
        IUserService userService
    )
        : base(userService)
    {
        _fileService = fileService;
        _customCommandService = customCommandService;
    }

    [HttpGet("file")]
    public IActionResult GetFileContent(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromQuery] string fileName
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);

        if (fileName.StartsWith(channelId))
        {
            fileName = fileName[(channelId.ToString().Length + 1)..];
        }

        var fileHeader = _fileService
            .GetFile(channelId, fileName)
            .OrThrow(() => new NotFoundException($"File {channelId}/{fileName} does not exist"));

        using var ms = new MemoryStream();
        _fileService.GetStreamForFile(channelId, fileName).CopyTo(ms);
        return File(ms.GetBuffer(), fileHeader.MimeType);
    }

    [HttpGet("sound-commands")]
    public SoundCommandAbstract[] GetSoundCommands(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        return _customCommandService
            .GetCommandsOfChannel(channelId)
            .Where(c => c.IsSoundCommand)
            .Select(c => new SoundCommandAbstract(
                c.Name,
                c.Responses.First(r => r.Type == ResponseType.Sound).Content
            ))
            .ToArray();
    }
}
