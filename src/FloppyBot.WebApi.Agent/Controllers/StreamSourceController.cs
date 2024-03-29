using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.FileStorage;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Dtos;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.Agent.Controllers;

[ApiController]
[Route("api/v1/stream-source/{messageInterface}/{channel}")]
[Authorize(Policy = "ApiKey")]
public class StreamSourceController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;
    private readonly IFileService _fileService;
    private readonly ICustomCommandService _customCommandService;

    public StreamSourceController(
        IFileService fileService,
        IApiKeyService apiKeyService,
        ICustomCommandService customCommandService
    )
    {
        _fileService = fileService;
        _apiKeyService = apiKeyService;
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
            .Where(V1CompatibilityProfile.IsConvertableForSoundCommand)
            .Select(c => new SoundCommandAbstract(
                c.Name,
                c.Responses.First(r => r.Type == ResponseType.Sound).Content
            ))
            .ToArray();
    }

    private ChannelIdentifier EnsureChannelAccess(string messageInterface, string channel)
    {
        var channelId = new ChannelIdentifier(messageInterface, channel);
        var apiKeyChannel = User.GetChannelIdForApiKeyUser();
        if (apiKeyChannel is null || channelId != apiKeyChannel)
        {
            throw new UnauthorizedException("You are not allowed to access this channel");
        }

        return channelId;
    }
}
