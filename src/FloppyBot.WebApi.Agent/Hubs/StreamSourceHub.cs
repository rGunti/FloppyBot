using FloppyBot.Commands.Custom.Communication.Entities;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace FloppyBot.WebApi.Agent.Hubs;

public class StreamSourceHub : Hub<IStreamSource>
{
    private readonly ILogger<StreamSourceHub> _logger;
    private readonly IApiKeyService _apiKeyService;

    public StreamSourceHub(ILogger<StreamSourceHub> logger, IApiKeyService apiKeyService)
    {
        _logger = logger;
        _apiKeyService = apiKeyService;
    }

    public async void Login(StreamSourceLoginArgs loginArgs)
    {
        _logger.LogDebug(
            "Connecting {ConnectionId} to Channel {ChannelId}",
            Context.ConnectionId,
            loginArgs.Channel
        );

        if (!_apiKeyService.ValidateApiKeyForChannel(loginArgs.Channel, loginArgs.Token))
        {
            _logger.LogWarning(
                "Invalid API key provided for channel {ChannelId}, terminating connection",
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
