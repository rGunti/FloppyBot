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
            .AddSingleton<DiscordConfiguration>(p => p.GetRequiredService<IConfiguration>()
                .GetSection("Discord")
                .Get<DiscordConfiguration>())
            // - Discord Client
            .AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient())
            // - Chat Interface
            .AddSingleton<IChatInterface, DiscordChatInterface>();
    }
}
