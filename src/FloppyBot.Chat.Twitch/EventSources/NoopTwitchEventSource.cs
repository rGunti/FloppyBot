using FloppyBot.Chat.Twitch.Events;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Chat.Twitch.EventSources;

public class NoopTwitchEventSource : ITwitchEventSource
{
    private readonly ILogger<NoopTwitchEventSource> _logger;
    public event EventHandler<TwitchChannelPointsRewardRedeemedEvent>? ChannelPointsCustomRewardRedemptionAdd;

    public NoopTwitchEventSource(ILogger<NoopTwitchEventSource> logger)
    {
        _logger = logger;
    }

    public Task ConnectAsync()
    {
        _logger.LogInformation(
            "Trying to connect a Noop Twitch Event Source. You will not get any events from Twitch."
        );
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        return Task.CompletedTask;
    }
}
