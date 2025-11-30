using FloppyBot.Chat.Twitch.Api;
using FloppyBot.Chat.Twitch.EventSources;
using FloppyBot.Chat.Twitch.Monitor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TwitchLib.Api;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;
using TwitchLib.Client;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Interfaces;
using TwitchLib.Communication.Models;
using TwitchLib.EventSub.Websockets.Extensions;

namespace FloppyBot.Chat.Twitch.Config;

public static class Registration
{
    public static IServiceCollection AddTwitchChatInterface(this IServiceCollection services)
    {
        return services
            // - Configuration
            .AddSingleton<TwitchConfiguration>(p =>
                p.GetRequiredService<IConfiguration>()
                    .GetSection("Twitch")
                    .Get<TwitchConfiguration>()
                ?? throw new ArgumentException("Twitch configuration is missing.")
            )
            .AddSingleton<ConnectionCredentials>(p =>
            {
                var config = p.GetRequiredService<TwitchConfiguration>();
                return new ConnectionCredentials(config.Username, config.Token);
            })
            .AddSingleton<ClientOptions>(p => new ClientOptions())
            // - Twitch Client
            .AddSingleton<IClient>(p => new WebSocketClient(p.GetRequiredService<ClientOptions>()))
            .AddSingleton<ITwitchClient>(p =>
            {
                var config = p.GetRequiredService<TwitchConfiguration>();
                var client = new TwitchClient(p.GetRequiredService<IClient>());
                client.Initialize(p.GetRequiredService<ConnectionCredentials>(), config.Channel);
                return client;
            })
            // - Twitch API
            .AddSingleton<ITwitchAPI>(p =>
            {
                var config = p.GetRequiredService<TwitchConfiguration>();
                var api = new TwitchAPI();
                if (config.HasTwitchApiCredentials)
                {
                    api.Settings.ClientId = config.ClientId;
                    api.Settings.Secret = config.AccessToken;
                }

                return api;
            })
            .AddSingleton<LiveStreamMonitorService>(p =>
            {
                var api = p.GetRequiredService<ITwitchAPI>();
                var config = p.GetRequiredService<TwitchConfiguration>();
                return new LiveStreamMonitorService(api, config.MonitorInterval);
            })
            .AddSingleton<ITwitchChannelOnlineMonitor, TwitchChannelOnlineMonitor>()
            .AddSingleton<ITwitchApiService, TwitchApiService>()
            // - Event Source
            .AddTwitchLibEventSubWebsockets()
            .AddSingleton<NoopTwitchEventSource>()
            .AddSingleton<TwitchEventSource>()
            .AddSingleton<ITwitchEventSource>(p =>
            {
                var config = p.GetRequiredService<TwitchConfiguration>();
                return config.EnableTwitchEventSource
                    ? p.GetRequiredService<TwitchEventSource>()
                    : p.GetRequiredService<NoopTwitchEventSource>();
            })
            // - Chat Interface
            .AddSingleton<IChatInterface, TwitchChatInterface>();
    }
}
