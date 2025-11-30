using FloppyBot.Chat.Twitch.Api;
using FloppyBot.Chat.Twitch.Config;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Chat.Twitch.Extensions;
using Microsoft.Extensions.Logging;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace FloppyBot.Chat.Twitch.EventSources;

public interface ITwitchEventSource
{
    event EventHandler<TwitchChannelPointsRewardRedeemedEvent> ChannelPointsCustomRewardRedemptionAdd;

    Task ConnectAsync();
    Task DisconnectAsync();
}

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

public class TwitchEventSource : ITwitchEventSource, IAsyncDisposable
{
    private readonly ILogger<TwitchEventSource> _logger;
    private readonly ITwitchApiService _twitchApi;
    private readonly EventSubWebsocketClient _client;
    private readonly TwitchConfiguration _configuration;

    public event EventHandler<TwitchChannelPointsRewardRedeemedEvent> ChannelPointsCustomRewardRedemptionAdd;

    public TwitchEventSource(
        ILogger<TwitchEventSource> logger,
        ITwitchApiService twitchApi,
        EventSubWebsocketClient client,
        TwitchConfiguration configuration
    )
    {
        _logger = logger;
        _client = client;
        _configuration = configuration;
        _twitchApi = twitchApi;

        _client.WebsocketConnected += (source, args) =>
        {
            try
            {
                ClientOnWebsocketConnected(source, args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in event handler ClientOnWebsocketConnected");
            }
        };
        _client.WebsocketDisconnected += ClientOnWebsocketDisconnected;
        _client.WebsocketReconnected += ClientOnWebsocketReconnected;
        _client.ErrorOccurred += ClientOnErrorOccurred;

        _client.ChannelPointsCustomRewardRedemptionAdd +=
            ClientOnChannelPointsCustomRewardRedemptionAdd;
    }

    public async Task ConnectAsync()
    {
        _logger.LogInformation("Connecting to Twitch");
        await _client.ConnectAsync();
    }

    public async Task DisconnectAsync()
    {
        _logger.LogInformation("Disconnecting from Twitch");
        await _client.DisconnectAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_client.SessionId))
        {
            await DisconnectAsync();
        }

        GC.SuppressFinalize(this);
    }

    private void ClientOnErrorOccurred(object? sender, ErrorOccuredArgs e)
    {
        _logger.LogError(e.Exception, "Twitch Event error: {ErrorMessage}", e.Message);
    }

    private async Task ClientOnWebsocketConnected(object? sender, WebsocketConnectedArgs e)
    {
        _logger.LogInformation("Connected to Twitch");
        if (e.IsRequestedReconnect)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Trying to establish channel point redemption subscription");
            await _twitchApi.CreateChannelPointsRedemptionSubscriptionAsync(
                _configuration.Channel,
                _client.SessionId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create channel point redemption subscription, no events will be received"
            );
        }
    }

    private void ClientOnWebsocketDisconnected(object? sender, EventArgs e)
    {
        _logger.LogWarning("Disconnected from Twitch");
    }

    private void ClientOnWebsocketReconnected(object? sender, EventArgs e)
    {
        _logger.LogInformation("Reconnected to Twitch");
    }

    private void ClientOnChannelPointsCustomRewardRedemptionAdd(
        object? sender,
        ChannelPointsCustomRewardRedemptionArgs e
    )
    {
        // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelchannel_points_custom_reward_redemptionadd
        var sourceEvent = e.Notification.Payload.Event;
        if (sourceEvent is null)
        {
            _logger.LogWarning(
                "No notification payload found for {Event}",
                e.Notification.Metadata.MessageId
            );
            return;
        }

        var eventData = sourceEvent.ConvertToInternalEvent();
        _logger.LogInformation("Channel Point Reward Redemption added: {@Event}", eventData);
        ChannelPointsCustomRewardRedemptionAdd?.Invoke(sender, eventData);
    }
}

internal static class InternalEventConverters
{
    internal static TwitchChannelPointsRewardRedeemedEvent ConvertToInternalEvent(
        this ChannelPointsCustomRewardRedemption redemption
    )
    {
        return new TwitchChannelPointsRewardRedeemedEvent(
            redemption.Id,
            TwitchEntityExtensions.ConvertToChatUser(redemption.UserLogin, redemption.UserName),
            new TwitchChannelPointsReward(
                redemption.Reward.Id,
                redemption.Reward.Title,
                redemption.Reward.Prompt,
                redemption.Reward.Cost
            )
        );
    }
}
