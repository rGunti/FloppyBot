using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Chat.Discord.Config;

public static class Registration
{
    public static IServiceCollection AddDiscordChatInterface(this IServiceCollection services)
    {
        return services
            // - Configuration
            .AddSingleton<DiscordConfiguration>(p =>
                p.GetRequiredService<IConfiguration>()
                    .GetSection("Discord")
                    .Get<DiscordConfiguration>()
                ?? throw new ArgumentException("Discord configuration missing")
            )
            // - Discord Client
            .AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient(
                new()
                {
                    GatewayIntents =
                        (
                            GatewayIntents.AllUnprivileged
                            & ~GatewayIntents.GuildScheduledEvents
                            & ~GatewayIntents.GuildInvites
                        ) | GatewayIntents.MessageContent,
                }
            ))
            // - Chat Interface
            .AddSingleton<IChatInterface, DiscordChatInterface>();
    }
}
