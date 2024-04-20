using FloppyBot.Commands.Custom.Communication.Entities;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace FloppyBot.WebApi.Agent.Hubs;

public class StreamSourceHub : Hub<IStreamSource>
{
    private readonly ILogger<StreamSourceHub> _logger;
    private readonly IApiKeyService _apiKeyService;
    private readonly IUserService _userService;

    public StreamSourceHub(
        ILogger<StreamSourceHub> logger,
        IApiKeyService apiKeyService,
        IUserService userService
    )
    {
        _logger = logger;
        _apiKeyService = apiKeyService;
        _userService = userService;
    }

    public async void Login(StreamSourceLoginArgs loginArgs)
    {
        _logger.LogDebug(
            "Connecting {ConnectionId} to Channel {ChannelId}",
            Context.ConnectionId,
            loginArgs.Channel
        );

        var apiKey = _apiKeyService.GetApiKey(loginArgs.Token);
        if (apiKey is null)
        {
            _logger.LogWarning(
                "API key {ApiKey} not found, terminating connection",
                loginArgs.Token
            );
            Context.Abort();
            return;
        }

        var accessibleChannelsForUser = _userService.GetAccessibleChannelsForUser(
            apiKey.OwnedByUser
        );
        if (!accessibleChannelsForUser.Contains(loginArgs.Channel))
        {
            _logger.LogWarning(
                "User {UserId} does not have access to channel {ChannelId}, terminating connection",
                apiKey.OwnedByUser,
                loginArgs.Channel
            );
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, loginArgs.Channel);
    }

    public async void Logout(string channelId)
    {
        _logger.LogDebug(
            "Disconnecting {ConnectionId} from Channel {ChannelId}",
            Context.ConnectionId,
            channelId
        );
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
    }

    private async void InvokeSoundCommand(SoundCommandInvocation soundCommandInvocation)
    {
        _logger.LogDebug("Invoking sound command");
        await Clients
            .Group(soundCommandInvocation.InvokedFrom)
            .InvokeSoundCommand(soundCommandInvocation);
    }
}

public interface IStreamSource
{
    Task InvokeSoundCommand(SoundCommandInvocation soundCommandInvocation);
}
