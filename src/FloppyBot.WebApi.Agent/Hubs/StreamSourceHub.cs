using FloppyBot.Commands.Custom.Communication.Entities;
using FloppyBot.WebApi.Base.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace FloppyBot.WebApi.Agent.Hubs;

public class StreamSourceHub : Hub<IStreamSource>
{
    private readonly ILogger<StreamSourceHub> _logger;

    public StreamSourceHub(ILogger<StreamSourceHub> logger)
    {
        _logger = logger;
    }

    public async void Login(StreamSourceLoginArgs loginArgs)
    {
        _logger.LogDebug(
            "Connecting {ConnectionId} to Channel {ChannelId}",
            Context.ConnectionId,
            loginArgs.Channel
        );
        // TODO: Validate token
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
