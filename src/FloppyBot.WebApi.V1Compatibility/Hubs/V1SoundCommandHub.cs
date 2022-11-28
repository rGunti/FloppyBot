// ReSharper disable UnusedMember.Global

using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FloppyBot.WebApi.V1Compatibility.Hubs;

public class V1SoundCommandHub : Hub<IV1SoundCommandHub>
{
    private readonly ILogger<V1SoundCommandHub> _logger;

    public V1SoundCommandHub(ILogger<V1SoundCommandHub> logger)
    {
        _logger = logger;
    }

    public async void Login(string channelId)
    {
        _logger.LogDebug("Connecting {ConnectionId} to Channel {ChannelId}",
            Context.ConnectionId, channelId);
        await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
    }

    public async void Logout(string channelId)
    {
        _logger.LogDebug("Disconnecting {ConnectionId} from Channel {ChannelId}",
            Context.ConnectionId, channelId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
    }

    public async void InvokeSoundCommand(InvokeSoundCommandEvent soundCommandEvent)
    {
        await Clients.Group(soundCommandEvent.InvokedFrom).InvokeSoundCommand(soundCommandEvent);
    }
}
