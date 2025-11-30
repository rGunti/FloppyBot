using FloppyBot.Chat.Twitch.Api;
using FloppyBot.Chat.Twitch.Config;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.TwitchApi.Storage;
using Microsoft.Extensions.Logging;
using TwitchLib.EventSub.Core.EventArgs.Channel;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;

namespace FloppyBot.Chat.Twitch.EventSources;

public class TwitchEventSource : ITwitchEventSource, IAsyncDisposable
{
    private readonly ILogger<TwitchEventSource> _logger;
    private readonly ITwitchApiService _twitchApi;
    private readonly EventSubWebsocketClient _client;
    private readonly TwitchConfiguration _configuration;
    private readonly ITwitchAccessCredentialsService _twitchCredentialsService;

    public event EventHandler<TwitchChannelPointsRewardRedeemedEvent> ChannelPointsCustomRewardRedemptionAdd;

    public TwitchEventSource(
        ILogger<TwitchEventSource> logger,
        ITwitchApiService twitchApi,
        EventSubWebsocketClient client,
        TwitchConfiguration configuration,
        ITwitchAccessCredentialsService twitchCredentialsService
    )
    {
        _logger = logger;
        _client = client;
        _configuration = configuration;
        _twitchCredentialsService = twitchCredentialsService;
        _twitchApi = twitchApi;

        _client.WebsocketConnected += ClientOnWebsocketConnected;
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

    private async Task ClientOnErrorOccurred(object? sender, ErrorOccuredArgs e)
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

    private async Task ClientOnWebsocketDisconnected(object? sender, EventArgs e)
    {
        _logger.LogWarning("Disconnected from Twitch");
    }

    private async Task ClientOnWebsocketReconnected(object? sender, EventArgs e)
    {
        _logger.LogInformation("Reconnected to Twitch");
    }

    private async Task ClientOnChannelPointsCustomRewardRedemptionAdd(
        object? sender,
        ChannelPointsCustomRewardRedemptionArgs e
    )
    {
        // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelchannel_points_custom_reward_redemptionadd
        var sourceEvent = e.Payload.Event;

        var eventData = sourceEvent.ConvertToInternalEvent();
        _logger.LogInformation("Channel Point Reward Redemption added: {@Event}", eventData);
        ChannelPointsCustomRewardRedemptionAdd?.Invoke(sender, eventData);
    }
}
