using System.Collections.Concurrent;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Chat.Twitch.Api.Dtos;
using FloppyBot.TwitchApi.Storage;
using FloppyBot.TwitchApi.Storage.Entities;
using Microsoft.Extensions.Logging;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Helix.Models.EventSub;
using TwitchLib.Api.Helix.Models.Teams;
using TwitchLib.Api.Interfaces;

namespace FloppyBot.Chat.Twitch.Api;

public interface ITwitchApiService
{
    IEnumerable<StreamTeam> GetStreamTeamsOfChannel(string channel);
    string? GetBroadcasterId(string channel);
    Task CreateChatMessageSubscriptionAsync(string channelName, string sessionId);
    Task CreateChannelPointsRedemptionSubscriptionAsync(string channelName, string sessionId);
}

public class TwitchApiService : ITwitchApiService, IAsyncDisposable
{
    private readonly ITwitchAPI _twitchApi;
    private readonly ILogger<TwitchApiService> _logger;
    private readonly ConcurrentDictionary<string, EventSubSubscription> _knownSubscriptions = new();
    private readonly ITwitchAccessCredentialsService _twitchAccessCredentials;

    public TwitchApiService(
        ITwitchAPI twitchApi,
        ILogger<TwitchApiService> logger,
        ITwitchAccessCredentialsService twitchAccessCredentials
    )
    {
        _twitchApi = twitchApi;
        _logger = logger;
        _twitchAccessCredentials = twitchAccessCredentials;
    }

    public IEnumerable<StreamTeam> GetStreamTeamsOfChannel(string channel)
    {
        return DoGetStreamTeamName(channel).GetAwaiter().GetResult();
    }

    public string? GetBroadcasterId(string channel)
    {
        return DoGetBroadcasterId(channel).GetAwaiter().GetResult();
    }

    public async Task CreateChatMessageSubscriptionAsync(string channelName, string sessionId)
    {
        var credentials = _twitchAccessCredentials.GetAccessCredentialsFor(
            new ChannelIdentifier(TwitchChatInterface.IF_NAME, channelName),
            "user:bot"
        );

        if (!credentials.HasValue)
        {
            _logger.LogError(
                "Failed to create channel points redemption subscription, no credential found for this channel"
            );
            return;
        }

        var channelId = await DoGetBroadcasterId(channelName);
        if (channelId is null)
        {
            return;
        }

        await CreateEventSubSubscription(
            "channel.chat.message",
            "1",
            new Dictionary<string, string>
            {
                { "broadcaster_user_id", channelId },
                { "user_id", channelId },
            },
            sessionId,
            credentials
        );
    }

    public async Task CreateChannelPointsRedemptionSubscriptionAsync(
        string channelName,
        string sessionId
    )
    {
        var credentials = _twitchAccessCredentials.GetAccessCredentialsFor(
            new ChannelIdentifier(TwitchChatInterface.IF_NAME, channelName),
            "channel:read:redemptions"
        );

        if (!credentials.HasValue)
        {
            _logger.LogError(
                "Failed to create channel points redemption subscription, no credential found for this channel"
            );
            return;
        }

        var channelId = await DoGetBroadcasterId(channelName);
        if (channelId is null)
        {
            return;
        }

        await CreateEventSubSubscription(
            "channel.channel_points_custom_reward_redemption.add",
            "1",
            new Dictionary<string, string> { { "broadcaster_user_id", channelId } },
            sessionId,
            credentials
        );
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var subscription in _knownSubscriptions.Values.ToList())
        {
            try
            {
                await DoUnsubscribe(subscription.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "Failed to unsubscribe from event subscription {SubscriptionId}",
                    subscription.Id
                );
            }
        }

        GC.SuppressFinalize(this);
    }

    private static StreamTeam ConvertToStreamTeam(Team team)
    {
        return new StreamTeam(team.Id, team.TeamName, team.TeamDisplayName);
    }

    private async Task<IEnumerable<StreamTeam>> DoGetStreamTeamName(string channel)
    {
        var broadcasterId = await DoGetBroadcasterId(channel);
        if (broadcasterId == null)
        {
            return Enumerable.Empty<StreamTeam>();
        }

        var teams = await _twitchApi.Helix.Teams.GetTeamsAsync(broadcasterId);
        return teams.Teams.Select(ConvertToStreamTeam);
    }

    private async Task<string?> DoGetBroadcasterId(string channel)
    {
        var program = await _twitchApi.Helix.Users.GetUsersAsync(logins: [channel]);
        return program.Users.Select(u => u.Id).FirstOrDefault();
    }

    private async Task<CreateEventSubSubscriptionResponse?> CreateEventSubSubscription(
        string eventType,
        string version,
        Dictionary<string, string> condition,
        string sessionId,
        TwitchAccessCredentials credentials
    )
    {
        _logger.LogDebug(
            "Subscribing to {SubscriptionEventType} with {SubscriptionSessionId}",
            eventType,
            sessionId
        );
        var result = await _twitchApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
            eventType,
            version,
            condition,
            EventSubTransportMethod.Websocket,
            sessionId,
            clientId: _twitchApi.Settings.ClientId,
            accessToken: credentials.AccessToken
        );

        if (result is null)
        {
            _logger.LogWarning(
                "Failed to subscribe to event {SubscriptionEventType} with {SubscriptionSessionId}, result was empty",
                eventType,
                sessionId
            );
            return null;
        }

        foreach (var subscription in result.Subscriptions)
        {
            _logger.LogDebug(
                "Registering / updating subscription {SubscriptionId}",
                subscription.Id
            );
            _knownSubscriptions.AddOrUpdate(
                subscription.Id,
                (_) => subscription,
                (_, _) => subscription
            );
        }

        return result;
    }

    private async Task<bool> DoUnsubscribe(string subscriptionId)
    {
        _logger.LogInformation("Unsubscribing {SubscriptionId}", subscriptionId);
        var result = await _twitchApi.Helix.EventSub.DeleteEventSubSubscriptionAsync(
            subscriptionId
        );

        if (result)
        {
            _knownSubscriptions.TryRemove(subscriptionId, out _);
        }

        return result;
    }
}
