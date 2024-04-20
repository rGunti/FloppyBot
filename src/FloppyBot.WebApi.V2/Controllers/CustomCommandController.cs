using FloppyBot.Commands.Custom.Storage;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.V2.Dtos.CustomCommands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers;

[ApiController]
[Route("api/v2/custom-commands/{messageInterface}/{channel}")]
[Authorize(Policy = Permissions.READ_CUSTOM_COMMANDS)]
public class CustomCommandController : ChannelScopedController
{
    private readonly ICustomCommandService _customCommandService;

    public CustomCommandController(
        IUserService userService,
        ICustomCommandService customCommandService
    )
        : base(userService)
    {
        _customCommandService = customCommandService;
    }

    [HttpGet]
    public List<CustomCommandDto> GetCommands(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        return _customCommandService
            .GetCommandsOfChannel(channelId)
            .Select(CustomCommandDto.FromEntity)
            .ToList();
    }
}
